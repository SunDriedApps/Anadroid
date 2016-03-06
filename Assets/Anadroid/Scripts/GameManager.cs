using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : RealTimeMultiplayerListener
{
    // game information
    const int QUICK_GAME_OPPONENTS = 1;
    const int GAME_VARIENT_VS = 0;
    const int MIN_OPPONENTS = 1;
    const int MAX_OPPONENTS = 1;
    const int MAX_SCORE_VS = 10;

    // message types
    const int MESSAGE_SCORE_UPDATE = 0;
    const int MESSAGE_INITIAL_ANAGRAM = 1;
    const int MESSAGE_ANAGRAM = 2;

    // categories
    const string CATEGORY_CAPITAL_CITIES = "Capital Cities";

    // category file names
    const string FILE_CAPITAL_CITIES = "CapitalCities";

    static GameManager sInstance = null;

    public enum GameState
    {
        SettingUp,
        Playing,
        Finished,
        SetupFailed,
        Aborted
    }

    private GameState mGameState = GameState.SettingUp;

    // participant ID's
    private string mId = "";
    private string mOpponentId = "";

    // player scores
    private int mScore = 0;
    private int mOpponentScore = 0;

    // is the current player the host
    private bool mIsHost = false;

    // all participants in the game
    private List<Participant> mParticipants;

    // container for the chosen category
    private CategoryContainer mCategoryContainer;

    // the chosen category for the game
    private string mChosenCategory;

    // the current anagram being solved
    public Anagram mCurrentAnagram;

    public GameManager()
    {
        // set the chosen category
        mChosenCategory = CATEGORY_CAPITAL_CITIES;
    }

    // send message to opponent
    byte[] mMessage = new byte[1];
    public void SendMessage(int messageType)
    {
        mMessage[0] = (byte) messageType;

        PlayGamesPlatform.Instance.RealTime.SendMessage(
            true, mOpponentId, mMessage);
    }

    // send anagram to opponent in the form of a byte array
    public void SendAnagram(int messageType, Anagram anagram)
    {
        // convert anagram to byte array
        byte[] anagramBytes = Anagram.ToByteArray(anagram);

        // the byte array to be sent
        byte[] message = new byte[anagramBytes.Length + 1];

        // insert message type in position 0
        message[0] = (byte) messageType;

        // insert anagram byte array after message type
        Buffer.BlockCopy(anagramBytes, 0, message, 1, anagramBytes.Length);

        // send the message to opponent
        PlayGamesPlatform.Instance.RealTime.SendMessage(
            true, mOpponentId, message);
    }

    public void OnRoomConnected(bool success)
    {
        if (success)
        {
            // store participants
            mParticipants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();

            // acquire participant ID's
            mId = GetSelf().ParticipantId;
            mOpponentId = GetOpponentId();

            // MAKING NEXUS HOST FOR TESTING ONLY
            if(!GetSelf().DisplayName.Equals("UndergroundSquid16"))
            {
                mIsHost = true;
            }

            // check if we are host
            if (mIsHost)
            {
                //mPlayerIsHost = true;

                // load category
                mCategoryContainer = CategoryContainer.Load(FILE_CAPITAL_CITIES);

                // get the first anagram
                mCurrentAnagram = mCategoryContainer.GetAnagram();

                // shuffle the current anagram
                mCurrentAnagram.Shuffle();

                // send anagram object to opponent
                SendAnagram(MESSAGE_INITIAL_ANAGRAM, mCurrentAnagram);

                mGameState = GameState.Playing;
            }

            Debug.Log("Room successfully connected.");
        }
        else
        {
            mGameState = GameState.SetupFailed;

            Debug.Log("Room setup failed");
        }
    }

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

                if(mIsHost)
                {
                    GetNextAnagram();
                }

                // has your opponent won the game?
                if (mOpponentScore == MAX_SCORE_VS)
                {
                    mGameState = GameState.Finished;
                }
                break;

            case MESSAGE_INITIAL_ANAGRAM:
                mCurrentAnagram = Anagram.FromByteArray(anagramInBytes);
                mGameState = GameState.Playing;
                break;

            case MESSAGE_ANAGRAM:
                mCurrentAnagram = Anagram.FromByteArray(anagramInBytes);
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
    // should only be used if we are hosting the game
    private void GetNextAnagram()
    {
        mCurrentAnagram = mCategoryContainer.GetAnagram();
        mCurrentAnagram.Shuffle();
        SendAnagram(MESSAGE_ANAGRAM, mCurrentAnagram);
    }

    // increment our score and send update to opponent
    public void IncrementScore()
    {
        mScore++;

        SendMessage(MESSAGE_SCORE_UPDATE);

        // have you won the game?
        if (mScore == MAX_SCORE_VS)
        {
            mGameState = GameState.Finished;
        }
        else if (mIsHost)
        {
            GetNextAnagram();
        }
    }

    public static void CreateQuickGame()
    {
        sInstance = new GameManager();
        PlayGamesPlatform.Instance.RealTime.CreateQuickGame(QUICK_GAME_OPPONENTS, QUICK_GAME_OPPONENTS,
            GAME_VARIENT_VS, sInstance);
    }

    public static void CreateWithInvitationScreen()
    {
        sInstance = new GameManager();
        PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen(MIN_OPPONENTS, MAX_OPPONENTS,
            GAME_VARIENT_VS, sInstance);
    }

    public static void AcceptFromInbox()
    {
        sInstance = new GameManager();
        PlayGamesPlatform.Instance.RealTime.AcceptFromInbox(sInstance);
    }

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
        Debug.Log("Peers disconnected");

        mGameState = GameState.Aborted;
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

    public Anagram GetCurrentAnagram
    {
        get
        {
            return mCurrentAnagram;
        }
    }

    public int GetScore
    {
        get
        {
            return mScore;
        }
    }

    public int GetOpponentScore
    {
        get
        {
            return mOpponentScore;
        }
    }

    public string GetChosenCatgeory
    {
        get
        {
            return mChosenCategory;
        }
    }
}
