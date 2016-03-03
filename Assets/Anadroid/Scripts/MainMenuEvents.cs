using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.BasicApi;

public class MainMenuEvents : MonoBehaviour {

    // To avoid processing events multiple times.
    private bool mProcessed = false;

    private System.Action<bool> mAuthCallback;
    private bool mSigningIn = false;

    // Use this for initialization
    void Start()
    {
        // a non silent authentication is when the user 
        // explicity signs into their account
        bool trySilentAuth = true;

        mAuthCallback = (bool success) =>
        {

            Debug.Log("In Auth callback, success = " + success);

            mSigningIn = false;
            if (success)
            {
                Debug.Log("Auth successful");
            }
            else
            {
                // authentication failed; try to explicity sign in
                trySilentAuth = false;

                Debug.Log("Auth failed!!");
            }
        };

        // build play games client
        var config = new PlayGamesClientConfiguration.Builder()
        .WithInvitationDelegate(InvitationManager.Instance.OnInvitationReceived)
        .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;

        // authorize player with google play servers
        Authorize(trySilentAuth);
    }

    //Starts the signin process.
    void Authorize(bool silent)
    {
        if (!mSigningIn)
        {
            Debug.Log("Starting sign-in...");
            PlayGamesPlatform.Instance.Authenticate(mAuthCallback, silent);
        }
        else
        {
            Debug.Log("Already started signing in");
        }
    }

    // called once per frame
    void Update()
    {

        UpdateInvitation();

        if (GameManager.Instance == null)
        {
            return;
        }

        Debug.Log("****** State = " + GameManager.Instance.State.ToString());

        switch (GameManager.Instance.State)
        {
            case GameManager.GameState.SettingUp:
                break;

            case GameManager.GameState.SetupFailed:
                GameManager.Instance.CleanUp();
                mProcessed = false;
                break;

            case GameManager.GameState.Aborted:
                GameManager.Instance.CleanUp();
                mProcessed = false;
                break;

            case GameManager.GameState.Finished:
                mProcessed = false;
                break;

            case GameManager.GameState.Playing:
                mProcessed = false;
                NavigationUtils.ShowGameScreen();
                break;
        }
    }

    // Handle detecting incoming invitations.
    public void UpdateInvitation()
    {
        if (InvitationManager.Instance == null)
        {
            return;
        }

        Invitation inv = InvitationManager.Instance.Invitation;
        if (inv != null)
        {
            if (InvitationManager.Instance.ShouldAutoAccept)
            {
                InvitationManager.Instance.Clear();
                GameManager.AcceptInvitation(inv.InvitationId);
            }
            else
            {
                // show the invitation screen
                NavigationUtils.ShowInvitationScreen();
            }
        }
    }

    public void OnQuickMatch()
    {
        if(mProcessed)
        {
            return;
        }
        mProcessed = true;

        GameManager.CreateQuickGame();
    }

    public void OnInvite()
    {
        if (mProcessed)
        {
            return;
        }
        mProcessed = true;

        GameManager.CreateWithInvitationScreen();
    }

    public void OnInbox()
    {
        mProcessed = true;

        GameManager.AcceptFromInbox();
    }

    public void OnSignOut()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CleanUp();
        }
        Debug.Log("Signing out...");
        if (PlayGamesPlatform.Instance != null)
        {
            PlayGamesPlatform.Instance.SignOut();
        }
        else
        {
            Debug.Log("PG Instance is null!");
        }
    }

}
