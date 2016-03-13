using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameScreenEvents : MonoBehaviour,  LetterOnEndDrag {

    const string MESSAGE_WINNER = "YOU WIN";
    const string MESSAGE_LOSER = "YOU LOSE";
    const string GAME_OBJECT_LETTER_GAP = "LetterGap";
    const string GAME_OBJECT_TIMER_BAR = "TimerBar";
    const string PREFAB_ANAGRAM_LETTER = "Letter";
    const int MAX_NUM_OF_HINTS = 2;
    const float TOTAL_TIME_TO_SOLVE_ANAGRAM = 40.0f;

    // game objects and components
    public Text score;
    public Text opponentScore;
    public GameObject timerContainer;
    private Image timerBar;
    public Text anagramCountText;
    public Text hintText;
    public GameObject anagramPanel;
    public GameObject dialogPanel;
    public GameObject gameOverDialog;
    public GameObject opponentDisconnectedDialog;
    private GridLayoutGroup anagramGrid;
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


    public void Start()
    {

        // disable standby on android device
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // get timer image
        timerBar = GameObject.Find(GAME_OBJECT_TIMER_BAR).GetComponent<Image>();

        mTimeRemaining = TOTAL_TIME_TO_SOLVE_ANAGRAM;

        UpdateTimer();

        if(GameManager.Instance == null)
        {
            return;
        }

        UpdateAnagramCountText();

        // get grid layouts from panels
        anagramGrid = anagramPanel.GetComponent<GridLayoutGroup>();

        // attempt to get the first anagram
        mCurrentAnagram = GameManager.Instance.CurrentAnagram;

        // add anagram to grid
        AddAnagramToGrid();
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
                timerContainer.SetActive(false);
                //gameOverDialog.SetActive(true);
                break;

            case GameManager.GameState.Aborted:
                timerContainer.SetActive(false);
                dialogPanel.SetActive(true);
                opponentDisconnectedDialog.SetActive(true);
                break;
        }

        // get a new anagram if we've ran out of time
        if (mTimeRemaining <= 0)
        {
            GameManager.Instance.GetNextAnagram();
        }

        UpdateTimer();

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
        }

        // update opponent score
        opponentScore.text = GameManager.Instance.OpponentScore.ToString();
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

            // send update to opponent
            GameManager.Instance.SendScoreUpdate();

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
}
