using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : RealTimeMultiplayerListener
{
    const int QUICK_GAME_OPPONENTS = 1;
    const int GAME_VARIENT_VS = 0;
    const int MIN_OPPONENTS = 1;
    const int MAX_OPPONENTS = 1;

    // categories
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
    private string mMyParticipantId = "";
    private string mOpponentId = "";

    // player scores
    private int mScore = 0;
    private int mOpponentScore = 0;

    // is the current player the host
    private bool mPlayerIsHost;

    // all participants in the game
    private List<Participant> mParticipants;

    // Container for the chosen category
    private CategoryContainer mCategoryContainer;


    public GameManager(){}

    byte[] mMessage = new byte[1];
    public void SendMessage(int message)
    {
        mMessage[0] = (byte)message;

        PlayGamesPlatform.Instance.RealTime.SendMessage(
            true, mOpponentId, mMessage);
    }

    private void LoadCategory(string filename)
    {
        mCategoryContainer = CategoryContainer.Load(FILE_CAPITAL_CITIES);
    }

    public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data)
    {
        
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

    public void OnRoomConnected(bool success)
    {
        if (success)
        {
            mGameState = GameState.Playing;

            // Store participants
            mParticipants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();

            // acquire participant ID's
            mMyParticipantId = GetSelf().ParticipantId;
            mOpponentId = GetOpponentId();

            // load game scene
            NavigationUtils.ShowGameScreen();

            Debug.Log("Room successfully connected. " + sInstance != null);
        }
        else
        {
            mGameState = GameState.SetupFailed;

            Debug.Log("Room failed to connect");
        }
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
        Debug.Log("Participant left the room");
    }

    public void OnPeersDisconnected(string[] peers)
    {
        Debug.Log("Peers disconnected");

        if (mGameState == GameState.Playing)
        {
            mGameState = GameState.Aborted;
        }
    }

    public void OnRoomSetupProgress(float percent)
    {
        
    }

    public void CleanUp()
    {
        PlayGamesPlatform.Instance.RealTime.LeaveRoom();
        mGameState = GameState.Aborted;
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
            if(!p.ParticipantId.Equals(mMyParticipantId))
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
}
