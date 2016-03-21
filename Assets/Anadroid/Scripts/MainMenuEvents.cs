using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.BasicApi;
using UnityEngine.UI;
using System;

/*
** A class to handle main menu events. We will attempt to sign the
** player into Google Play Games once this script is started
*/

public class MainMenuEvents : MonoBehaviour
{
    const string SIGN_IN_BUTTON_TEXT = "SIGN IN";
    const string SIGN_OUT_BUTTON_TEXT = "SIGN OUT";
    const string SOUND_EFFECTS_DISABLED_IMAGE = "SoundEffectsDisabledImage";
    const string MUSIC_DISABLED_IMAGE = "MusicDisabledImage";

    public GameObject dialogPanel;
    public GameObject signInFailedDialog;
    public GameObject searchingDialog;
    public GameObject invitationDialog;
    public GameObject musicSideMenuButton;
    public GameObject soundEffectsSideMenuButton;
    public Text invatationDialogPlayerName;
    public Text signInButtonText;
    public AudioSource menuButtonAudioSource;
    public AudioSource musicAudioSource;

    // To avoid processing events multiple times.
    private bool mProcessed = false;

    // a callback to detect whether the sign in process was successful
    private System.Action<bool> mAuthCallback;

    // are we currently signed into the play games servers?
    private static bool sSignedIn = false;

    private bool mMusicEnabled;

    private bool mSoundEffectsEnabled;

    private static bool sMusicStarted = false;


    void Awake()
    {
        // load audio preferences
        int pref = PlayerPrefs.GetInt(GameManager.MUSIC_ENABLED_KEY, 1);
        mMusicEnabled = Convert.ToBoolean(pref);
        GameObject.Find(MUSIC_DISABLED_IMAGE).GetComponent<Image>().enabled = !mMusicEnabled;

        pref = PlayerPrefs.GetInt(GameManager.SOUND_EFFECTS_ENABLED_KEY, 1);
        mSoundEffectsEnabled = Convert.ToBoolean(pref);
        GameObject.Find(SOUND_EFFECTS_DISABLED_IMAGE).GetComponent<Image>().enabled = !mSoundEffectsEnabled;

        Debug.Log("Music started = " + sMusicStarted);

        DontDestroyOnLoad(musicAudioSource);

        if (!sMusicStarted)
        {
            musicAudioSource.Play();

            if (!mMusicEnabled)
            {
                musicAudioSource.mute = true;
            }

            sMusicStarted = true;
        }
    }

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if(GameManager.Instance != null)
        {
            GameManager.Instance.CleanUp();
        }

        if(sSignedIn)
        {
            signInButtonText.text = SIGN_OUT_BUTTON_TEXT;

            return;
        }

        mAuthCallback = (bool success) =>
        {
            Debug.Log("In Auth callback, success = " + success);

            if (success)
            {
                signInButtonText.text = SIGN_OUT_BUTTON_TEXT;
                sSignedIn = true;
                Debug.Log("Auth successful");
            }

            else
            {
                dialogPanel.SetActive(true);
                signInFailedDialog.SetActive(true);
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
        Authorize(false);
    }

    //Starts the signin process.
    void Authorize(bool silent)
    {
        if(sSignedIn == true)
        {
            Debug.Log("Already signed in!");
            return;
        }

        PlayGamesPlatform.Instance.Authenticate(mAuthCallback, silent);
        Debug.Log("Starting sign-in...");
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

            case GameManager.GameState.PreGame:
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

        Invitation invite = InvitationManager.Instance.Invitation;
        if (invite != null)
        {
            if (InvitationManager.Instance.ShouldAutoAccept)
            {
                InvitationManager.Instance.Clear();
                GameManager.AcceptInvitation(invite.InvitationId);
            }
            else
            {
                invatationDialogPlayerName.text = invite.Inviter.DisplayName;
                dialogPanel.SetActive(true);
                invitationDialog.SetActive(true);
            }
        }
    }

    public void OnQuickMatch()
    {
        if (mProcessed)
        {
            return;
        }

        mProcessed = true;

        dialogPanel.SetActive(true);
        searchingDialog.SetActive(true);

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

    // Sign in/Sign out button click 
    public void OnSignOutClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CleanUp();
        }

        if(sSignedIn)
        {
            Debug.Log("Signing out...");
            if (PlayGamesPlatform.Instance != null)
            {
                PlayGamesPlatform.Instance.SignOut();

                signInButtonText.text = SIGN_IN_BUTTON_TEXT;
            }
            else
            {
                Debug.Log("PG Instance is null!");
            }
            sSignedIn = false;
        }
        else
        {
            Authorize(false);
        }
    }

    // sign in failed dialog button click
    public void OnSignInFailedClick()
    {
        dialogPanel.SetActive(false);
        signInFailedDialog.SetActive(false);
    }

    // stop searching when searching panel is clicked
    public void OnSearchingDialogClick()
    {
        dialogPanel.SetActive(false);
        searchingDialog.SetActive(false);
        GameManager.Instance.CleanUp();
        mProcessed = false;
    }

    public void OnAcceptInvitationClick()
    {
        Invitation invite = InvitationManager.Instance.Invitation;
        if (invite == null)
        {
            Debug.Log("No Invite!");
            invitationDialog.SetActive(false);
            dialogPanel.SetActive(false);
            return;
        }

        InvitationManager.Instance.Clear();
        GameManager.AcceptInvitation(invite.InvitationId);
        invitationDialog.SetActive(false);
        dialogPanel.SetActive(false);
        Debug.Log("Invitation accepted");
    }

    public void OnDeclineInvitationClick()
    {
        InvitationManager.Instance.DeclineInvitation();
        invitationDialog.SetActive(false);
        dialogPanel.SetActive(false);
        Debug.Log("Invitation declined");
    }

    public void OnMusicClick()
    {
        mMusicEnabled = !mMusicEnabled;

        // store preference
        PlayerPrefs.SetInt(GameManager.MUSIC_ENABLED_KEY, Convert.ToInt32(mMusicEnabled));

        musicAudioSource.mute = !mMusicEnabled;

        GameObject.Find(MUSIC_DISABLED_IMAGE).GetComponent<Image>().enabled = !mMusicEnabled;
    }

    public void OnSoundEffectsClick()
    {
        mSoundEffectsEnabled = !mSoundEffectsEnabled;

        // store preference
        PlayerPrefs.SetInt(GameManager.SOUND_EFFECTS_ENABLED_KEY, Convert.ToInt32(mSoundEffectsEnabled));

        menuButtonAudioSource.enabled = mSoundEffectsEnabled;

        GameObject.Find(SOUND_EFFECTS_DISABLED_IMAGE).GetComponent<Image>().enabled = !mSoundEffectsEnabled;
    }
}
