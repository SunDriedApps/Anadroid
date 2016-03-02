using UnityEngine;
using System.Collections;
using GooglePlayGames.BasicApi.Multiplayer;

public class InvitationScreenEvents : MonoBehaviour {

    // the invitation object being processed.
    private Invitation mInvite;

    private bool mProcessed = false;

    // Update is called once per frame
    void Update()
    {

        mInvite = (mInvite != null) ? mInvite : InvitationManager.Instance.Invitation;
        if (mInvite == null && !mProcessed)
        {
            Debug.Log("No Invite -- back to main");
            NavigationUtils.ShowMainMenu();
            return;
        }

        if (GameManager.Instance != null)
        {
            switch (GameManager.Instance.State)
            {
                case GameManager.GameState.Aborted:
                    Debug.Log("Aborted -- back to main");
                    NavigationUtils.ShowMainMenu();
                    break;

                case GameManager.GameState.Finished:
                    Debug.Log("Finished-- back to main");
                    NavigationUtils.ShowMainMenu();
                    break;

                case GameManager.GameState.Playing:
                    NavigationUtils.ShowGameScreen();
                    break;

                case GameManager.GameState.SettingUp:
                    break;

                case GameManager.GameState.SetupFailed:
                    Debug.Log("Failed -- back to main");
                    NavigationUtils.ShowMainMenu();
                    break;
            }
        }
    }

    // Handler script for the Accept button.  This method should be added
    // to the On Click list for the accept button.
    public void OnAccept()
    {

        if (mProcessed)
        {
            return;
        }

        mProcessed = true;
        InvitationManager.Instance.Clear();

        GameManager.AcceptInvitation(mInvite.InvitationId);
        Debug.Log("Accepted! RaceManager state is now " + GameManager.Instance.State);

    }

    // Handler script for the decline button.
    public void OnDecline()
    {

        if (mProcessed)
        {
            return;
        }

        mProcessed = true;
        InvitationManager.Instance.DeclineInvitation();

        NavigationUtils.ShowMainMenu();
    }
}
