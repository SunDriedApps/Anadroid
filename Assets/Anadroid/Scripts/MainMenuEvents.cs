using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.BasicApi;

public class MainMenuEvents : MonoBehaviour {

    // To avoid processing events multiple times.
    private bool mProcessed = false;

    private System.Action<bool> mAuthCallback;
    private bool mAuthOnStart = true;
    private bool mSigningIn = false;

    // Use this for initialization
    void Start()
    {
        CategoryContainer cc = CategoryContainer.Load("CapitalCities");
        foreach (Anagram a in cc.mAnagrams)
        {
            Debug.Log(a.GetSolution);
        }

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
                Debug.Log("Auth failed!!");
            }
        };

        // enable debug logs
        var config = new PlayGamesClientConfiguration.Builder()
        .WithInvitationDelegate(InvitationManager.Instance.OnInvitationReceived)
        .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;

        // try silent authentication
        if (mAuthOnStart)
        {
            Authorize(true);
        }

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

            default:
                Debug.Log("RaceManager.Instance.State = " + GameManager.Instance.State);
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
