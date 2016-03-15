using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class Test : MonoBehaviour, LetterOnEndDrag {
    const string PREFAB_ANAGRAM_LETTER = "Letter";
    const string FILE_CAPITAL_CITIES = "CapitalCities";
    const string FILE_TEST_CATEGORY = "testCategory";
    const string LETTER_GAP_NAME = "LetterGap";
    const string GAME_OBJECT_TIMER_BAR = "TimerBar";
    const int MAX_NUM_OF_HINTS = 1;
    const float TIME_TO_SOLVE = 20.0f;

    public Text hintText;
    public GameObject anagramPanel;
    public GameObject anagramPanel2;
    public GameObject hintLifeBubble;
    public GameObject shuffleLifeBubble;
    public GameObject timeContainer;
    private Image timerBar;

    private HorizontalLayoutGroup anagramGrid;

    private HorizontalLayoutGroup anagramGrid2;
    
    private Text letterText;

    private CategoryContainer mCategoryContainer;

    private static Anagram mCurrentAnagram;

    private int mNumOfUsedHints = 0;

    private float mTimeRemaining;

    // Use this for initialization
    void Start ()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        anagramGrid = anagramPanel.GetComponent<HorizontalLayoutGroup>();

        anagramGrid2 = anagramPanel2.GetComponent<HorizontalLayoutGroup>();
        
        mCategoryContainer = CategoryContainer.Load(FILE_CAPITAL_CITIES);

        timerBar = GameObject.Find(GAME_OBJECT_TIMER_BAR).GetComponent<Image>();

        Debug.Log(timerBar.name);

        UpdateCurrentAnagram();

        mTimeRemaining = TIME_TO_SOLVE;

        UpdateTime();
    }

    void Update()
    {
        if(mTimeRemaining <= 0)
        {
            Debug.Log("Time up");
            UpdateCurrentAnagram();
            ResetPanel(anagramPanel);
        }

        UpdateTime();
    }

    // decrement time and update text
    private void UpdateTime()
    {
        mTimeRemaining -= Time.deltaTime;

        // update time display
        timerBar.fillAmount = mTimeRemaining / TIME_TO_SOLVE;
    }

    private void ResetTime()
    {
        mTimeRemaining = TIME_TO_SOLVE;
    }

    private bool AnagramSolved()
    {
        Debug.Log("AnagramSolved called. Child count = " + anagramPanel.transform.childCount);

        char letter;
        int index;
        bool pastLetterGap = false;

        // check every character against the solution
        for (int i = 0; i < anagramPanel.transform.childCount; i++)
        {
            if(anagramPanel.transform.GetChild(i).name == LETTER_GAP_NAME)
            {
                pastLetterGap = true;
                continue;
            }
            
            if(pastLetterGap)
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

    // get new anagram and add it to the anagram panel
    private void UpdateCurrentAnagram()
    {
        // get new anagram
        mCurrentAnagram = mCategoryContainer.GetAnagram();

        mCurrentAnagram.Shuffle();

        AddAnagramToGrid();
    }

    private void AddAnagramToGrid()
    {
        // iterate through each character and make a letter prefab
        for (int i = 0; i < mCurrentAnagram.Length; i++)
        {
            // instantiate a letter prefab from the resources folder
            GameObject letter = (GameObject)Instantiate(Resources.Load(PREFAB_ANAGRAM_LETTER));

            // set text component of letter
            letterText = letter.GetComponentInChildren<Text>();
            letterText.text = mCurrentAnagram.Shuffled[i].ToString();

            letter.GetComponent<Draggable>().SetOnEndDrag(this);

            // checks for word length exceeding panel max, if less add to first
            if (i < mCurrentAnagram.Length)
            {
                // add letter to the anagram panel
                AddLetterToGrid(letter,anagramGrid);
            }

            // if index greateer than first panel add to second panel
            else
            {
                AddLetterToGrid(letter, anagramGrid2);
            }

        }
    }

    // add letter to anagram grid
    private void AddLetterToGrid(GameObject letter, HorizontalLayoutGroup nonEmptyPanel)
    {
        letter.transform.SetParent(nonEmptyPanel.transform, false);
        letter.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        letter.transform.localPosition = Vector3.zero;
    }

    // destroy letters in grid
    private void ResetPanel(GameObject gridPanel)
    {
        for (int i = 0; i < anagramPanel.transform.childCount; i++)
        {
            Debug.Log(anagramPanel.transform.GetChild(i).name);
            Destroy(anagramPanel.transform.GetChild(i).gameObject);
        }
    }

    public void OnHintLifeBubbleClick()
    {
        mNumOfUsedHints++;

        hintText.text = mCurrentAnagram.Hint;

        DisableLifeBubble(hintLifeBubble);
    }

    // re-shuffle
    public void OnShuffleLifeBubbleClick()
    {
        mCurrentAnagram.Shuffle();
        ResetPanel(anagramPanel);
        AddAnagramToGrid();
    }

    private void DisableLifeBubble(GameObject lifeBubble)
    {
        lifeBubble.GetComponentInChildren<Button>().interactable = false;

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

    public void OnEndDrag()
    {
        if (AnagramSolved())
        {
            Debug.Log("Solved!");

            ResetPanel(anagramPanel);
            UpdateCurrentAnagram();
        }
        else
        {
            Debug.Log("Not solved");
        }
    }
}
