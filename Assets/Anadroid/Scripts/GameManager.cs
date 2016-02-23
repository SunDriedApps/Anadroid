using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
using System;
using System.Collections.Generic;

public class GameManager : RealTimeMultiplayerListener
{
    const int QUICK_GAME_OPPONENTS = 1;
    const int GAME_VARIENT_VS = 0;
    const int MIN_OPPONENTS = 1;
    const int MAX_OPPONENTS = 3;

    static GameManager sInstance = null;

    public enum GameState
    {
        SettingUp,
        Playing,
        Finished,
        SetupFailed,
        Aborted
    }

    ;

    private GameState mGameState = GameState.SettingUp;

    // my participant ID
    private string mMyParticipantId = "";

    // room setup progress
    private float mRoomSetupProgress = 0.0f;

    float mRoomSetupStartTime = 0.0f;

    private GameManager()
    {
        mRoomSetupStartTime = Time.time;
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
            mMyParticipantId = GetSelf().ParticipantId;

            NavigationUtils.ChangeScene(1);

            Debug.Log("Room successfully connected");
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

        // if, as a result, we are the only player in the race, it's over
        if (mGameState == GameState.Playing)
        {
            mGameState = GameState.Aborted;
        }
    }

    public void OnRoomSetupProgress(float percent)
    {
        mRoomSetupProgress = percent;
    }

    public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data)
    {
        int score = (int)data[1];
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

    private Participant GetParticipant(string participantId)
    {
        return PlayGamesPlatform.Instance.RealTime.GetParticipant(participantId);
    }
}
