using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
using System;

public class GameManager : RealTimeMultiplayerListener
{
    public void OnLeftRoom()
    {
        throw new NotImplementedException();
    }

    public void OnParticipantLeft(Participant participant)
    {
        throw new NotImplementedException();
    }

    public void OnPeersConnected(string[] participantIds)
    {
        throw new NotImplementedException();
    }

    public void OnPeersDisconnected(string[] participantIds)
    {
        throw new NotImplementedException();
    }

    public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data)
    {
        throw new NotImplementedException();
    }

    public void OnRoomConnected(bool success)
    {
        throw new NotImplementedException();
    }

    public void OnRoomSetupProgress(float percent)
    {
        throw new NotImplementedException();
    }
}
