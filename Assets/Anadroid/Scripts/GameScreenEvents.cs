using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameScreenEvents : MonoBehaviour, LetterOnEndDrag {

    const string MESSAGE_WINNER = "YOU WIN";
    const string MESSAGE_LOSER = "YOU LOSE";
    const string GAME_OBJECT_LETTER_GAP = "LetterGap";
    const string GAME_OBJECT_TIMER_BAR = "TimerBar";
    const string GAME_OBJECT_READY_BUTTON = "ReadyUpButton";
    const string GAME_OBJECT_READY_UP_BUTTON_TEXT = "ReadyUpButtonText";
    const string GAME_OBJECT_PRE_GAME_MESSAGE = "PreGameMessageText";
    const string GAME_OBJECT_PRE_GAME_CATEGORY = "GameCategoryText";
    const string GAME_OBJECT_GAME_TYPE = "GameTypeContentText";
    const string GAME_OBJECT_PRE_GAME_OBJECTIVE = "ObjectiveContentText";
    const string GAME_OBJECT_GAME_RESULT_TEXT = "GameResultText";
    const string GAME_OBJECT_FINAL_SCORE_TEXT = "FinalScoreText";
    const string PREFAB_ANAGRAM_LETTER = "Letter";
    const string READY_TEXT = "READY";
    const string WAITING_FOR_OPPONENT_TEXT = "Waiting for opponent";
    const string GAME_OBJECTIVE_VS = "Score more points that your opponent";
    const string GAME_RESULT_WIN = "You Win";
    const string GAME_RESULT_LOSS = "You Lose";
    const string GAME_RESULT_DRAW = "Draw";
    const int MAX_NUM_OF_HINTS = 2;
    const float TOTAL_TIME_TO_SOLVE_ANAGRAM = 25.0f;

    // game objects and components
    public Text score;
    public Text opponentScore;
    public GameObject timerContainer;
    private Image timerBar;
    public Text anagramCountText;
    public Text hintText;
    public GameObject anagramPanel;
    public GameObject dialogPanel;
    public GameObject preGameDialog;
    public GameObject postGameDialog;
    public GameObject opponentDisconnectedDialog;
    private HorizontalLayoutGroup anagramGrid;
    public GameObject shuffleLifeBubble;
    public GameObject hintLifeBubble;
    public GameObject revealLifeBubble;

    // the current anagram being solved
    private Anagram mCurrentAnagram;

    // the number of hints used in the 
    private int mNumOfUsedHints = 0;

    // only 1 re-shuffle allowed per anagram
    private bool mUsedShuffle = false;

    // time remaining to solve the current anagram
    private float mTimeRemaining;

    private bool mTimeUp = false;

    // have we clicked ready
    private bool mReady = false;

    // used whenever we need to reference a text component from a game object
    private Text reusableText;


    public void Start()
    {

        // disable standby on android device
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // get timer image
        timerBar = GameObject.Find(GAME_OBJECT_TIMER_BAR).GetComponent<Image>();

        mTimeRemaining = TOTAL_TIME_TO_SOLVE_ANAGRAM;

        UpdateAnagramCountText();

        // get grid layouts from panels
        anagramGrid = anagramPanel.GetComponent<HorizontalLayoutGroup>();

        // attempt to get the first anagram
        mCurrentAnagram = GameManager.Instance.CurrentAnagram;

        // set pre game dialog information
        reusableText = GameObject.Find(GAME_OBJECT_PRE_GAME_CATEGORY).GetComponent<Text>();
        reusableText.text = GameManager.Instance.Catgeory;

        reusableText = GameObject.Find(GAME_OBJECT_GAME_TYPE).GetComponent<Text>();
        reusableText.text = GameManager.Instance.GameType;

        reusableText = GameObject.Find(GAME_OBJECT_PRE_GAME_OBJECTIVE).GetComponent<Text>();
        reusableText.text = GetGameObjective();
    }

    // returns the game result in a string
    private string GetGameResult()
    {
        int score = GameManager.Instance.Score;
        int opponentScore = GameManager.Instance.OpponentScore;

        if (score > opponentScore)
        {
            return GAME_RESULT_WIN;
        }
        else if(opponentScore > score)
        {
            return GAME_RESULT_LOSS;
        }
        else
        {
            return GAME_RESULT_DRAW;
        }
    }

    // returns the final score as a string
    private string GetFinalScore()
    {
        return GameManager.Instance.Score.ToString() + " - " +
            GameManager.Instance.OpponentScore;
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
                if(dialogPanel.activeSelf)
                {
                    return;
                }

                // disable timer
                timerContainer.SetActive(false);

                // show post game dialog
                dialogPanel.SetActive(true);
                postGameDialog.SetActive(true);

                // set game result text component
                reusableText = GameObject.Find(GAME_OBJECT_GAME_RESULT_TEXT).GetComponent<Text>();
                reusableText.text = GetGameResult();

                // set final score text
                reusableText = GameObject.Find(GAME_OBJECT_FINAL_SCORE_TEXT).GetComponent<Text>();
                reusableText.text = GetFinalScore();
                return;

            case GameManager.GameState.Aborted:
                
                // return if post game dialog already showing
                if (dialogPanel.activeSelf)
                {
                    return;
                }

                timerContainer.SetActive(false);
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

        // both players are ready so get the first anagram
        else if (GameManager.Instance.State == GameManager.GameState.PreGame)
        {

            // attempt to get the first anagram
            mCurrentAnagram = GameManager.Instance.CurrentAnagram;

            // add anagram to grid
            AddAnagramToGrid();

            UpdateAnagramCountText();

            // hide pre game dialog
            dialogPanel.SetActive(false);
            preGameDialog.SetActive(false);

            GameManager.Instance.SetState(GameManager.GameState.Playing);

            Debug.Log("Game started");
        }

        // get a new anagram if we've ran out of time
        if (mTimeRemaining <= 0 && !mTimeUp)
        {
            Debug.Log("Time up");
            mTimeUp = true;
            GameManager.Instance.IncrementAnagramCount();
            GameManager.Instance.GetNextAnagram();
        }

        if(GameManager.Instance.State == GameManager.GameState.Playing)
        {
            UpdateTimer();
        }

        // check with the game manager to see if we've moved onto a new anagram
        if (!mCurrentAnagram.Equals(GameManager.Instance.CurrentAnagram)) {

            Debug.Log("Moving onto new anagram");
            
            // reset anagram panel
            ResetPanel(anagramPanel);

            // clear hint text
            hintText.text = "";

            mCurrentAnagram = GameManager.Instance.CurrentAnagram;

            // re-enable hint life bubble if we haven't used max amount
            if (mNumOfUsedHints != MAX_NUM_OF_HINTS)
            {
                EnableLifeBubble(hintLifeBubble);
            }

            if(mUsedShuffle)
            {
                mUsedShuffle = false;
                EnableLifeBubble(shuffleLifeBubble);
            }

            AddAnagramToGrid();

            ResetTimer();

            // update anagram count text
            UpdateAnagramCountText();

            if(mTimeUp)
            {
                mTimeUp = false;
            }
        }

        // update opponent score
        opponentScore.text = GameManager.Instance.OpponentScore.ToString();
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

    // decrement time and update text
    private void UpdateTimer()
    {
        mTimeRemaining -= Time.deltaTime;

        // update time display
        timerBar.fillAmount = mTimeRemaining / TOTAL_TIME_TO_SOLVE_ANAGRAM;
    }

    private void ResetTimer()
    {
        mTimeRemaining = TOTAL_TIME_TO_SOLVE_ANAGRAM;
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

        for(int i = 0; i < anagramPanel.transform.childCount; i++)
        {
            Destroy(anagramPanel.transform.GetChild(i).gameObject);
        }
    }

    // used to get reference to every letter's text component
    private Text letterText;

    // add the current anagram to the grid
    private void AddAnagramToGrid()
    {
        if(mCurrentAnagram == null)
        {
            Debug.Log("AddAnagramToGrid() returning early current anagram = null");
            return;
        }

        // iterate through each character and make a letter prefab
        for (int i = 0; i < mCurrentAnagram.Length; i++)
        {
            // instantiate a letter prefab from the resources folder
            GameObject letter = (GameObject)Instantiate(Resources.Load(PREFAB_ANAGRAM_LETTER));

            // set text component of letter
            letterText = letter.GetComponentInChildren<Text>();
            letterText.text = mCurrentAnagram.Shuffled[i].ToString();

            // set the LetterOnEndDrag interface in each letter's draggable script
            // to this script in order to OnEndDrag after the letter
            letter.GetComponent<Draggable>().SetOnEndDrag(this);

            // add letter to the anagram panel
            AddLetterToGrid(letter);
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
    public void OnEndDrag()
    {
        Debug.Log("OnPoinerUp Called");

        if (AnagramSolved())
        {
            Debug.Log("Solved!");

            // increment score
            GameManager.Instance.IncrementScore();

            // update anagram count
            GameManager.Instance.IncrementAnagramCount();

            // send update to opponent
            GameManager.Instance.SendMessage(GameManager.MESSAGE_SCORE_UPDATE);

            // update onscreen score
            score.text = GameManager.Instance.Score.ToString();
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

        mCurrentAnagram.Shuffle();

        ResetPanel(anagramPanel);

        AddAnagramToGrid();

        DisableLifeBubble(shuffleLifeBubble);
    }

    // show a hint for the current anagram
    public void OnHintLifeBubbleClick()
    {
        mNumOfUsedHints++;

        hintText.text = mCurrentAnagram.Hint;

        DisableLifeBubble(hintLifeBubble);
    }

    // reveal 2 letters from the solution
    public void OnRevealLifeBubbleClick()
    {
        
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

    // an onclick method assigned through the unity editor
    public void OnBackToMainScreenClicked()
    {
        NavigationUtils.ShowMainMenu();
    }

    // set the anagram count text to display what number anagram
    // we are currently on out of the total number of anagrams
    private void UpdateAnagramCountText()
    {
        anagramCountText.text = GameManager.Instance.AnagramCount.ToString() +
            "/" + GameManager.Instance.MaxAnagrams.ToString();
    }

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
}
