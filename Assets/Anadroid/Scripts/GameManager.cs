﻿using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
using System;
using System.Collections.Generic;
using UnityEngine.UI;


/*
** A singleton class which contains game state information and handles communication
** between players. The GameManager will be reset everytime a new game is created
*/
public class GameManager : RealTimeMultiplayerListener
{
    // pref keys
    public const string MUSIC_ENABLED_KEY = "MusicEnabled";
    public const string SOUND_EFFECTS_ENABLED_KEY = "SoundEffectsEnabled";
    public const string GAME_RESULT_WIN = "You Win";
    public const string GAME_RESULT_LOSS = "You Lose";
    public const string GAME_RESULT_DRAW = "Draw";
    public const float ANAGRAM_TRANSITION_TIME = 2f;

    // game information
    const int QUICK_GAME_OPPONENTS = 1;
    const int GAME_VARIENT_VS = 0;
    const int MIN_OPPONENTS = 1;
    const int MAX_OPPONENTS = 1;

    // message types
    public const int MESSAGE_SCORE_UPDATE = 0;
    public const int MESSAGE_ANAGRAM = 2;
    public const int MESSAGE_OPPONENT_READY = 3;

    // categories; used for title on game screen
    const string CATEGORY_CAPITAL_CITIES = "Capital Cities";

    // category file names
    const string FILE_CAPITAL_CITIES = "CapitalCities";

    // game types
    public const string GAME_TYPE_VS = "VS";

    // the singleton instance
    static GameManager sInstance = null;

    // the set of possible game states
    public enum GameState
    {
        SettingUp,
        PreGame,
        Playing,
        Finished,
        SetupFailed,
        Aborted
    }

    // set the current game state
    private GameState mGameState = GameState.SettingUp;

    // participant ID's
    private string mId = "";
    private string mOpponentId = "";

    // player scores
    private int mScore = 0;
    private int mOpponentScore = 0;

    // custom red
    private Color mRed;

    // is the current player the host
    private bool mWeAreHost = false;

    // all participants in the game
    private List<Participant> mParticipants;

    // container for the chosen category
    private CategoryContainer mCategoryContainer;

    // the chosen category for the game
    private string mCategory;

    // the chosen gametype for the game
    private string mGameType;

    // the current anagram being solved
    private Anagram mCurrentAnagram;

    // has your opponent clicked ready
    private bool mOpponentReady = false;

    private bool mMusicEnabled;
    private bool mSoundEffectsEnabled;


    public GameManager()
    {
        // set the chosen category
        mCategory = CATEGORY_CAPITAL_CITIES;

        mGameType = GAME_TYPE_VS;

        mMusicEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1));
        mSoundEffectsEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(SOUND_EFFECTS_ENABLED_KEY, 1));

        mRed = new Color(229f / 255f, 65f / 255f, 73f / 255f);
    }

    // send message to opponent
    byte[] mMessage = new byte[1];
    public void SendMessage(int messageType)
    {
        mMessage[0] = (byte) messageType;

        PlayGamesPlatform.Instance.RealTime.SendMessage(
            true, mOpponentId, mMessage);
    }

    // returns the game result in a string
    public string GetGameResult()
    {
        if (mScore > mOpponentScore)
        {
            return GAME_RESULT_WIN;
        }
        else if (mOpponentScore > mScore)
        {
            return GAME_RESULT_LOSS;
        }
        else
        {
            return GAME_RESULT_DRAW;
        }
    }

    // send anagram to opponent in the form of a byte array
    public void SendAnagram(Anagram anagram)
    {
        // convert anagram to byte array
        byte[] anagramBytes = Anagram.ToByteArray(anagram);

        // the byte array to be sent
        byte[] message = new byte[anagramBytes.Length + 1];

        // insert message type in position 0
        message[0] = (byte) MESSAGE_ANAGRAM;

        // insert anagram byte array after message type
        Buffer.BlockCopy(anagramBytes, 0, message, 1, anagramBytes.Length);

        // send the message to opponent
        PlayGamesPlatform.Instance.RealTime.SendMessage(
            true, mOpponentId, message);
    }

    // handle what happens once the players are connected
    public void OnRoomConnected(bool success)
    {
        if (success)
        {
            // store participants
            mParticipants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();

            // acquire participant ID's
            mId = GetSelf().ParticipantId;
            mOpponentId = GetOpponentId();

            // first participant in the list is made host
            if(mParticipants[0].ParticipantId.Equals(mId))
            {
                mWeAreHost = true;
            }

            // check if we are host
            if (mWeAreHost)
            {
                // load category
                mCategoryContainer = CategoryContainer.Load(FILE_CAPITAL_CITIES);

                // get the first anagram
                mCurrentAnagram = mCategoryContainer.GetAnagram();

                // shuffle the current anagram
                mCurrentAnagram.Shuffle();

                // send anagram object to opponent
                SendAnagram(mCurrentAnagram);
            }

            mGameState = GameState.PreGame;

            Debug.Log("Room successfully connected.");
        }
        else
        {
            mGameState = GameState.SetupFailed;

            Debug.Log("Room setup failed");
        }
    }

    // handle any messages recived from your opponent
    public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data)
    {
        Debug.Log("Message recieved of type " + data[0].ToString());

        // get message type from data
        int messageType = data[0];

        // copy anagram bytes from data
        byte[] anagramInBytes = new byte[data.Length - 1];
        Buffer.BlockCopy(data, 1, anagramInBytes, 0, anagramInBytes.Length);

        switch(messageType)
        {
            case MESSAGE_SCORE_UPDATE:
                mOpponentScore++;
                GetNextAnagram();
                break;

            case MESSAGE_ANAGRAM:
                mCurrentAnagram = Anagram.FromByteArray(anagramInBytes);
                break;

            case MESSAGE_OPPONENT_READY:
                mOpponentReady = true;
                break;
        }
    }

    // load category into category container
    // only the host will do this
    private void LoadCategory(string filename)
    {
        mCategoryContainer = CategoryContainer.Load(FILE_CAPITAL_CITIES);
    }

    // get the next anagram and send to opponent
    public void GetNextAnagram()
    {

        // if we aren't hosting return and wait 
        // for the next anagram to be sent instead
        if (!mWeAreHost)
        {
            return;
        }

        // get next anagram
        mCurrentAnagram = mCategoryContainer.GetAnagram();
        mCurrentAnagram.Shuffle();

        // send to opponent
        SendAnagram(mCurrentAnagram);
    }

    // increment our score and send update to opponent
    public void IncrementScore()
    {
        mScore++;

        if(mGameState != GameState.Finished)
        {
            GetNextAnagram();
        }
    }

    // search for a quick game
    public static void CreateQuickGame()
    {
        sInstance = new GameManager();
        PlayGamesPlatform.Instance.RealTime.CreateQuickGame(QUICK_GAME_OPPONENTS, QUICK_GAME_OPPONENTS,
            GAME_VARIENT_VS, sInstance);
    }

    // invite a friend to play
    public static void CreateWithInvitationScreen()
    {
        sInstance = new GameManager();
        PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen(MIN_OPPONENTS, MAX_OPPONENTS,
            GAME_VARIENT_VS, sInstance);
    }

    // check your inbox for invitations to play
    public static void AcceptFromInbox()
    {
        sInstance = new GameManager();
        PlayGamesPlatform.Instance.RealTime.AcceptFromInbox(sInstance);
    }

    // accept an invitation to play
    public static void AcceptInvitation(string invitationId)
    {
        sInstance = new GameManager();
        PlayGamesPlatform.Instance.RealTime.AcceptInvitation(invitationId, sInstance);
    }

    public void OnLeftRoom()
    {
        if (mGameState != GameState.Finished)
        {
            mGameState = GameState.Aborted;
        }
    }

    public void OnPeersConnected(string[] peers)
    {
        Debug.Log("Peer connected");
    }

    public void OnParticipantLeft(Participant participant)
    {
        mGameState = GameState.Aborted;

        Debug.Log("Participant left the room");
    }

    public void OnPeersDisconnected(string[] peers)
    {
        mGameState = GameState.Aborted;

        Debug.Log("Peers disconnected");
    }

    public void OnRoomSetupProgress(float percent)
    {
        
    }

    public void CleanUp()
    {
        PlayGamesPlatform.Instance.RealTime.LeaveRoom();
        sInstance = null;
    }

    private Participant GetSelf()
    {
        return PlayGamesPlatform.Instance.RealTime.GetSelf();
    }

    private String GetOpponentId()
    {
        foreach(Participant p in mParticipants)
        {
            if(!p.ParticipantId.Equals(mId))
            {
                return p.ParticipantId;
            }
        }

        return null;
    }

    private Participant GetParticipant(string participantId)
    {
        return PlayGamesPlatform.Instance.RealTime.GetParticipant(participantId);
    }

    public void SetState(GameState state)
    {
        mGameState = state;
    }

    public GameState State
    {
        get
        {
            return mGameState;
        }
    }

    public static GameManager Instance
    {
        get
        {
            return sInstance;
        }
    }

    public Anagram CurrentAnagram
    {
        get
        {
            return mCurrentAnagram;
        }
    }

    public int Score
    {
        get
        {
            return mScore;
        }
    }

    public int OpponentScore
    {
        get
        {
            return mOpponentScore;
        }
    }

    public string Category
    {
        get
        {
            return mCategory;
        }
    }

    public bool IsHost
    {
        get
        {
            return mWeAreHost;
        }
    }

    public bool OpponentReady
    {
        get
        {
            return mOpponentReady;
        }
    }

    public string GameType
    {
        get
        {
            return mGameType;
        }
    }

    public bool MusicEnabled
    {
        get
        {
            return mMusicEnabled;
        }
    }

    public bool SoundEffectsEnabled
    {
        get
        {
            return mSoundEffectsEnabled;
        }
    }

    public Color CustomRed
    {
        get
        {
            return mRed;
        }
    }
}
