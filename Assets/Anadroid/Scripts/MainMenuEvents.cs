using UnityEngine;
using System.Collections;

public class MainMenuEvents : MonoBehaviour {

    // To avoid processing events multiple times.
    private bool mProcessed = false;

    public void OnQuickMatch()
    {
        Debug.Log("Quick game clicked");

        if(mProcessed)
        {
            return;
        }
        mProcessed = true;

        GameManager.CreateQuickGame();
    }

    public void OnInvite()
    {
        Debug.Log("Invite Clicked");

        if (mProcessed)
        {
            return;
        }
        mProcessed = true;

        GameManager.CreateWithInvitationScreen();
    }

    public void OnInbox()
    {
        Debug.Log("Inbox Clicked");
    }

    public void OnSignOut()
    {
        Debug.Log("Sign out Clicked");
    }

}
