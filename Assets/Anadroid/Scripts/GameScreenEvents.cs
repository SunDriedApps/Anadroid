using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

/*
** A class to handle any game screen events and game logic. This class will
** communicate with the GameManager which will allow proper communication
** between players
*/

public class GameScreenEvents : MonoBehaviour, LetterOnEndDrag {

    const string GAME_OBJECT_LETTER_GAP = "LetterGap";
    const string GAME_OBJECT_READY_BUTTON = "ReadyUpButton";
    const string GAME_OBJECT_READY_UP_BUTTON_TEXT = "ReadyUpButtonText";
    const string GAME_OBJECT_PRE_GAME_MESSAGE = "PreGameMessageText";
    const string GAME_OBJECT_GAME_TYPE = "GameTypeContentText";
    const string GAME_OBJECT_PRE_GAME_OBJECTIVE = "ObjectiveContentText";
    const string GAME_OBJECT_GAME_RESULT_TEXT = "GameResultText";
    const string GAME_OBJECT_FINAL_SCORE_TEXT = "FinalScoreText";
    const string PREFAB_ANAGRAM_LETTER = "Letter";
    const string READY_TEXT = "READY";
    const string WAITING_FOR_OPPONENT_TEXT = "Waiting for opponent";
    const string GAME_OBJECTIVE_VS = "Gain a point for every anagram solved. The player with the most points at the end of the game wins";
    const int MAX_NUM_OF_UNLOCKS = 2;
    const int MAX_NUM_OF_SHUFFLES = 2;

    // game objects and components
    public Text category;
    public Text hintText;
    public GameObject mainPanel;
    public GameObject anagramPanel;
    public GameObject dialogPanel;
    public GameObject opponentDisconnectedDialog;
    public GameObject preGameDialog;
    public GameObject postGameDialog;
    public GameObject homeDialog;
    public Text shuffleCountText;
    public Text unlockCountText;
    private HorizontalLayoutGroup anagramGrid;
    public GameObject shuffleLifeBubble;
    public GameObject revealLifeBubble;
    public AudioSource lifeBubbleAudio;
    public AudioSource winAudio;
    public AudioSource loseAudio;
    public AudioSource drawAudio;
    public AudioSource newAnagramAudio;

    // the current anagram being solved
    private Anagram mCurrentAnagram;

    // a reference to all letter objects currently on screen
    private List<GameObject> mLetters;

    // number of reveals used
    private int mUnlocksLeft = MAX_NUM_OF_UNLOCKS;

    // number of shuffles used
    private int mShufflesLeft = MAX_NUM_OF_SHUFFLES;

    private bool mUsedShuffle = false;

    // have we clicked ready
    private bool mReady = false;

    // used whenever we need to reference a text component from a game object
    private Text reusableText;

    // audio preferences
    private bool mSoundEffectsEnabled;
    private bool mMusicEnabled;

    public void Start()
    {
        // disable standby on android device
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // set life bubble uses text
        shuffleCountText.text = MAX_NUM_OF_SHUFFLES.ToString();
        unlockCountText.text = MAX_NUM_OF_UNLOCKS.ToString();

        mMusicEnabled = GameManager.Instance.MusicEnabled;

        mSoundEffectsEnabled = GameManager.Instance.SoundEffectsEnabled;

        Debug.Log(mMusicEnabled + " " + mSoundEffectsEnabled);

        mLetters = new List<GameObject>();

        // get grid layouts from panels
        anagramGrid = anagramPanel.GetComponent<HorizontalLayoutGroup>();

        // show pre game dialog
        dialogPanel.SetActive(true);
        preGameDialog.SetActive(true);

        category.text = GameManager.Instance.Category;

        reusableText = GameObject.Find(GAME_OBJECT_GAME_TYPE).GetComponent<Text>();
        reusableText.text = GameManager.Instance.GameType;

        reusableText = GameObject.Find(GAME_OBJECT_PRE_GAME_OBJECTIVE).GetComponent<Text>();
        reusableText.text = GetGameObjective();
    }

    public void Update()
    {

        if(GameManager.Instance == null)
        {
            return;
        }

        switch(GameManager.Instance.State)
        {
            case GameManager.GameState.Finished:

                // return if post game dialog already showing
                if(postGameDialog.activeSelf)
                {
                    return;
                }

                StartCoroutine(DisplayPostGameDialog());

                return;

            case GameManager.GameState.Aborted:
                
                // return if post game dialog already showing
                if (dialogPanel.activeSelf)
                {
                    return;
                }

                dialogPanel.SetActive(true);
                opponentDisconnectedDialog.SetActive(true);
                return;
        }

        // if either player isn't ready to play return
        if (!mReady || !GameManager.Instance.OpponentReady)
        {
            Debug.Log("Players not ready");
            return;
        }

        // if pre game dialog is showing and both players are ready, get the first anagram
        else if (GameManager.Instance.State == GameManager.GameState.PreGame)
        {

            // attempt to get the first anagram
            mCurrentAnagram = GameManager.Instance.CurrentAnagram;

            hintText.text = mCurrentAnagram.Hint;

            // add anagram to grid
            AddAnagramToGrid();

            // hide pre game dialog
            dialogPanel.SetActive(false);
            preGameDialog.SetActive(false);

            GameManager.Instance.SetState(GameManager.GameState.Playing);

            Debug.Log("Game started");
        }


        // check with the game manager to see if we've moved onto a new anagram
        if (!mCurrentAnagram.Equals(GameManager.Instance.CurrentAnagram))
        {

            Debug.Log("Moving onto new anagram");

            // clear hint text
            hintText.text = "";

            mCurrentAnagram = GameManager.Instance.CurrentAnagram;

            // re-enable hint life bubble if we haven't used max amount
            if (mShufflesLeft != 0)
            {
                EnableLifeBubble(shuffleLifeBubble);
            }

            mUsedShuffle = false;

            if(mUnlocksLeft != 0)
            {
                EnableLifeBubble(revealLifeBubble);
            }

            StartCoroutine(AnagramTransition());
        }
    }

    // co-routine to display post game dialog
    IEnumerator DisplayPostGameDialog()
    {
        postGameDialog.SetActive(true);

        yield return new WaitForSeconds(GameManager.ANAGRAM_TRANSITION_TIME);

        // show post game dialog
        dialogPanel.SetActive(true);

        // set game result text component
        reusableText = GameObject.Find(GAME_OBJECT_GAME_RESULT_TEXT).GetComponent<Text>();

        string gameResult = GameManager.Instance.GetGameResult();
        reusableText.text = gameResult;

        if (GameManager.Instance.SoundEffectsEnabled)
        {
            switch (gameResult)
            {
                case GameManager.GAME_RESULT_WIN:
                    winAudio.Play();
                    break;
                case GameManager.GAME_RESULT_LOSS:
                    loseAudio.Play();
                    break;
                case GameManager.GAME_RESULT_DRAW:
                    drawAudio.Play();
                    break;
            }
        }

        // set final score text
        reusableText = GameObject.Find(GAME_OBJECT_FINAL_SCORE_TEXT).GetComponent<Text>();
        reusableText.text = GetFinalScore();
    }

    // co-routine called between anagrams
    IEnumerator AnagramTransition()
    {
        TimerController.DisableTimer();

        yield return new WaitForSeconds(GameManager.ANAGRAM_TRANSITION_TIME);

        anagramPanel.SetActive(false);

        // reset anagram panel
        ResetPanel(anagramPanel);

        AddAnagramToGrid();

        // set the new hint
        hintText.text = mCurrentAnagram.Hint;

        TimerController.EnableTimer();
    }

    // returns the final score as a string
    private string GetFinalScore()
    {
        return GameManager.Instance.Score.ToString() + " - " +
            GameManager.Instance.OpponentScore;
    }

    // return a string describing the game type objective
    private string GetGameObjective()
    {
        switch (GameManager.Instance.GameType)
        {
            case GameManager.GAME_TYPE_VS:
                return GAME_OBJECTIVE_VS;
            default:
                return null;
        }
    }

    private bool AnagramSolved()
    {
        char letter;
        int index;
        bool pastLetterGap = false;

        // check every character against the solution
        for (int i = 0; i < anagramPanel.transform.childCount; i++)
        {
            if (anagramPanel.transform.GetChild(i).name == GAME_OBJECT_LETTER_GAP)
            {
                pastLetterGap = true;
                continue;
            }

            if (pastLetterGap)
            {
                index = i - 1;
            }
            else
            {
                index = i;
            }

            // get letter from game object
            letter = char.Parse(anagramPanel.transform.GetChild(i)
                .GetComponentInChildren<Text>().text);

            // letter doesnt match with solution
            if (letter != mCurrentAnagram.Solution[index])
            {
                return false;
            }
        }

        // all letters match with solution
        return true;
    }

    // destroy letters in grid
    private void ResetPanel(GameObject gridPanel)
    {
        Debug.Log("Resetting panel");

        foreach (GameObject letter in mLetters)
        {
            Destroy(letter);
        }

        mLetters.Clear();

        for (int i = 0; i < anagramPanel.transform.childCount; i++)
        {
            Destroy(anagramPanel.transform.GetChild(i).gameObject);
        }
    }

    // add the current anagram to the grid
    private void AddAnagramToGrid()
    {
        if(mCurrentAnagram == null)
        {
            Debug.Log("AddAnagramToGrid() returning early current anagram = null");
            return;
        }

        if(GameManager.Instance.State == GameManager.GameState.Finished)
        {
            return;
        }

        // iterate through each character and make a letter prefab
        for (int i = 0; i < mCurrentAnagram.Length; i++)
        {
            // instantiate a letter prefab from the resources folder
            GameObject letter = (GameObject)Instantiate(Resources.Load(PREFAB_ANAGRAM_LETTER));

            // set text component of letter
            reusableText = letter.GetComponentInChildren<Text>();
            reusableText.text = mCurrentAnagram.Shuffled[i].ToString();

            // set the LetterOnEndDrag interface in each letter's draggable script
            // to this script in order to OnEndDrag after the letter
            letter.GetComponent<LetterBehaviour>().SetOnEndDrag(this);

            // add letter to the anagram panel
            AddLetterToGrid(letter);

            // keep a reference to this letter by adding it to the list
            mLetters.Add(letter);
        }

        anagramPanel.SetActive(true);

        if (mSoundEffectsEnabled && !mUsedShuffle)
        {
            newAnagramAudio.Play();
        }
    }

    // add letter to anagram grid
    private void AddLetterToGrid(GameObject letter)
    {
        letter.transform.SetParent(anagramGrid.transform, false);
        letter.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        letter.transform.localPosition = Vector3.zero;
    }

    // implementing the LetterOnEndDrag interface
    // this method will be called directly after LetterBehaviour.OnEndDrag
    public void OnEndDrag()
    {
        Debug.Log("OnPoinerUp Called");

        if (AnagramSolved())
        {
            Debug.Log("Solved!");
           
            // increment score
            GameManager.Instance.IncrementScore();

            // send update to opponent
            GameManager.Instance.SendMessage(GameManager.MESSAGE_SCORE_UPDATE);
        }
        else
        {
            Debug.Log("Not solved");
        }
    }

    // re-shuffle
    public void OnShuffleLifeBubbleClick()
    {
        mUsedShuffle = true;

        mShufflesLeft--;

        shuffleCountText.text = mShufflesLeft.ToString();

        mCurrentAnagram.Shuffle();

        ResetPanel(anagramPanel);

        AddAnagramToGrid();

        DisableLifeBubble(shuffleLifeBubble);
    }

    // unlock 2 letters from the solution
    public void OnUnlockLifeBubbleClick()
    {
        mUnlocksLeft--;

        unlockCountText.text = mUnlocksLeft.ToString();

        // get random index for first letter
        int randomLetterIndex1;
        randomLetterIndex1 = Random.Range(0, mCurrentAnagram.Length);

        // get random index for second letter
        int randomLetterIndex2 = Random.Range(0, mCurrentAnagram.Length);
        while (randomLetterIndex1 == randomLetterIndex2)
        {
            randomLetterIndex2 = Random.Range(0, mCurrentAnagram.Length);
        }

        // use random indexes to get two chars from solution
        char solutionLetter1 = mCurrentAnagram.Solution[randomLetterIndex1];
        char solutionLetter2 = mCurrentAnagram.Solution[randomLetterIndex2];

        UnlockLetterTile(solutionLetter1, randomLetterIndex1);
        UnlockLetterTile(solutionLetter2, randomLetterIndex2);

        DisableLifeBubble(revealLifeBubble);
    }

    // find a letter tile that matches the solution letter and
    // swap it for the tile at the given index
    private void UnlockLetterTile(char solutionLetter, int index)
    {
        GameObject solutionLetterTile = GetLetterTile(solutionLetter);
        GameObject shuffledLetterTile = anagramGrid.transform.GetChild(index).gameObject;
        if (!solutionLetterTile.Equals(shuffledLetterTile))
        {
            SwapLetterTile(solutionLetterTile, shuffledLetterTile);
        }

        SetLetterTileComponentsLocked(index);
    }

    // returns the first letter that matches the given string
    private GameObject GetLetterTile(char c)
    {
        foreach (GameObject letter in mLetters)
        {
            if (letter.GetComponentInChildren<Text>().text.Equals(c.ToString()))
            {
                return letter;
            }
        }

        return null;
    }

    GameObject reusableLetterTile;

    // used to hold a reference to all of a letters image components
    Image[] letterComponents;

    // set letter tile as locked in the draggable script
    // enable lock image on tile
    private void SetLetterTileComponentsLocked(int index)
    {
        reusableLetterTile = anagramGrid.transform.GetChild(index).gameObject;

        reusableLetterTile.GetComponent<LetterBehaviour>().SetLocked();

        // get reference to all letter components
        letterComponents = reusableLetterTile.GetComponentsInChildren<Image>();

        // find the lock image and enable it
        foreach (Image image in letterComponents)
        {
            if (image.name.Equals("LockImage"))
            {
                image.enabled = true;
            }
        }
    }

    // swap two letter tiles
    private void SwapLetterTile(GameObject letter1, GameObject letter2)
    {
        int letter1Index = letter1.transform.GetSiblingIndex();

        // set letter 2 in letter 1's place
        letter1.transform.SetSiblingIndex(letter2.transform.GetSiblingIndex());

        // set letter 1 in letter 2's place
        letter2.transform.SetSiblingIndex(letter1Index);
    }

    private void DisableLifeBubble(GameObject lifeBubble)
    {
        // disable button
        lifeBubble.GetComponentInChildren<Button>().interactable = false;

        // change icon colour
        Image icon = GameObject.Find(lifeBubble.name + "/Icon").GetComponent<Image>();
        icon.color = new Color(200, 200, 200, 0.4f);
    }

    private void EnableLifeBubble(GameObject lifeBubble)
    {
        // disable button
        lifeBubble.GetComponentInChildren<Button>().interactable = true;

        // change icon colour
        Image icon = GameObject.Find(lifeBubble.name + "/Icon").GetComponent<Image>();
        icon.color = Color.white;
    }

    // navigate back to the main screen
    public void OnBackToMainScreenClicked()
    {
        NavigationUtils.ShowMainMenu();
    }

    // players will click this button in the pregame dialog indicating they are ready to play
    public void OnReadyUpClick()
    {
        Debug.Log("Ready clicked");

        // we're ready to play
        mReady = true;

        // let your opponent know you're ready to play
        GameManager.Instance.SendMessage(GameManager.MESSAGE_OPPONENT_READY);

        // update ready button text
        reusableText = GameObject.Find(GAME_OBJECT_READY_UP_BUTTON_TEXT).GetComponent<Text>();
        reusableText.text = READY_TEXT;

        // disable button
        GameObject readyButton = GameObject.Find(GAME_OBJECT_READY_BUTTON);
        readyButton.GetComponent<Button>().interactable = false;
        Color transparentWhite = Color.white;
        transparentWhite.a = 0.4f;
        readyButton.GetComponentInChildren<Text>().color = transparentWhite;

        // update message
        reusableText = GameObject.Find(GAME_OBJECT_PRE_GAME_MESSAGE).GetComponent<Text>();
        reusableText.text = WAITING_FOR_OPPONENT_TEXT;
    }

    public void OnHomeButtonClick()
    {
        dialogPanel.SetActive(true);
        homeDialog.SetActive(true);
    }

    public void CloseDialogPanel()
    {
        homeDialog.SetActive(false);
        dialogPanel.SetActive(false);
    }
}
