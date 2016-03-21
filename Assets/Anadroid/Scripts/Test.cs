using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour, LetterOnEndDrag {
    const string PREFAB_ANAGRAM_LETTER = "Letter";
    const string FILE_CAPITAL_CITIES = "CapitalCities";
    const string FILE_TEST_CATEGORY = "testCategory";
    const string LETTER_GAP_NAME = "LetterGap";
    const string GAME_OBJECT_TIMER_BAR = "TimerBar";
    const string GAME_OBJECT_READY_BUTTON = "ReadyUpButton";
    const string GAME_OBJECT_READY_UP_BUTTON_TEXT = "ReadyUpButtonText";
    const string GAME_OBJECT_PRE_GAME_MESSAGE = "PreGameMessageText";
    const string GAME_OBJECT_PRE_GAME_CATEGORY = "GameCategoryText";
    const string GAME_OBJECT_GAME_TYPE = "GameTypeContentText";
    const string GAME_OBJECT_PRE_GAME_OBJECTIVE = "ObjectiveContentText";
    const int MAX_NUM_OF_HINTS = 1;
    const float TIME_TO_SOLVE = 100.0f;

    public Text hintText;
    public GameObject anagramPanel;
    public GameObject hintLifeBubble;
    public GameObject shuffleLifeBubble;
    public GameObject revealLifeBubble;
    public Text timerText;
    public AudioSource correctAudioSource;
    public AudioSource timeOutAudioSource;
    public AudioSource timeAlmostUp;

    private HorizontalLayoutGroup anagramGrid;

    private Text letterText;

    private CategoryContainer mCategoryContainer;

    private static Anagram mCurrentAnagram;

    private List<GameObject> mLetters = new List<GameObject>();

    private int mNumOfUsedHints = 0;

    private float mTimeRemaining;

    private GameObject reusableLetterTile;

    // Use this for initialization
    void Start ()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        anagramGrid = anagramPanel.GetComponent<HorizontalLayoutGroup>();

        mCategoryContainer = CategoryContainer.Load(FILE_CAPITAL_CITIES);

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
        timerText.text = mTimeRemaining.ToString("0");

        // warn player time is almost up
        if(mTimeRemaining < 9)
        {
            Debug.Log("Time almost up");
            timerText.color = new Color(229f/255f, 65f/255f, 73f/255f);
        }
    }

    private void ResetTime()
    {
        mTimeRemaining = TIME_TO_SOLVE;

        timerText.color = Color.white;
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

            letter.GetComponent<LetterBehaviour>().SetOnEndDrag(this);

            AddLetterToGrid(letter);

            mLetters.Add(letter);
        }
    }

    // add letter to anagram grid
    private void AddLetterToGrid(GameObject letter)
    {
        letter.transform.SetParent(anagramGrid.transform, false);
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

    // returns the first letter that matches the given string
    private GameObject GetLetterTile(char c)
    {
        foreach(GameObject letter in mLetters)
        {
            if (letter.GetComponentInChildren<Text>().text.Equals(c.ToString()))
            {
                return letter;
            }
        }

        return null;
    }

    // reveal 2 letters from the solution
    public void OnRevealLifeBubbleClick()
    {
        // get random index for first letter
        int randomLetterIndex1;
        randomLetterIndex1 = Random.Range(0, mCurrentAnagram.Length);

        // get random index for second letter
        int randomLetterIndex2 = Random.Range(0, mCurrentAnagram.Length);
        while(randomLetterIndex1 == randomLetterIndex2)
        {
            randomLetterIndex2 = Random.Range(0, mCurrentAnagram.Length);
        }

        // use random indexes to get two chars from solution
        char solutionLetter1 = mCurrentAnagram.Solution[randomLetterIndex1];
        char solutionLetter2 = mCurrentAnagram.Solution[randomLetterIndex2];

        RevealLetterTile(solutionLetter1, randomLetterIndex1);
        RevealLetterTile(solutionLetter2, randomLetterIndex2); 
    }

    // find a letter tile that matches the solution letter and
    // swap it for the tile at the given index
    private void RevealLetterTile(char solutionLetter, int index)
    {
        GameObject solutionLetterTile = GetLetterTile(solutionLetter);
        GameObject shuffledLetterTile = anagramGrid.transform.GetChild(index).gameObject;
        if (!solutionLetterTile.Equals(shuffledLetterTile))
        {
            SwapLetterTile(solutionLetterTile, shuffledLetterTile);
        }

        SetLetterTileComponentsLocked(index);
    }
    
    
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
}
