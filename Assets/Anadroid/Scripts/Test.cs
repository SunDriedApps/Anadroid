using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class Test : MonoBehaviour, LetterOnPointerUp {
    const string PREFAB_ANAGRAM_LETTER = "Letter";
    const string FILE_CAPITAL_CITIES = "CapitalCities";
    const string LETTER_GAP_NAME = "LetterGap";
    const int MAX_NUM_OF_HINTS = 1;

    public Text hintText;
    public GameObject anagramPanel;
    public GameObject hintLifeBubble;
    public GameObject shuffleLifeBubble;

    private GridLayoutGroup anagramGrid;

    private Text letterText;

    private CategoryContainer mCategoryContainer;

    private static Anagram mCurrentAnagram;

    private static bool mTimeUp = false;

    private int mNumOfUsedHints = 0;

    // Use this for initialization
    void Start ()
    {
        anagramGrid = anagramPanel.GetComponent<GridLayoutGroup>();

        mCategoryContainer = CategoryContainer.Load(FILE_CAPITAL_CITIES);

        UpdateCurrentAnagram();
    }

    void Update()
    {
        if(mTimeUp)
        {
            Debug.Log("Time up");
            UpdateCurrentAnagram();
            mTimeUp = false;
        }
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
            if (letter != mCurrentAnagram.GetSolution[index])
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
            letterText.text = mCurrentAnagram.GetShuffled[i].ToString();

            letter.GetComponent<Draggable>().SetOnPointerUp(this);

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

    // destroy letters in grid
    private void ResetGrid(GameObject gridPanel)
    {
        for(int i = 0; i < anagramPanel.transform.childCount; i++)
        {
            Debug.Log(anagramPanel.transform.GetChild(i).name);
            Destroy(anagramPanel.transform.GetChild(i).gameObject);
        }
    }

    public static void TimeUp()
    {
        mTimeUp = true;
    }

    public void OnHintLifeBubbleClick()
    {
        mNumOfUsedHints++;

        hintText.text = mCurrentAnagram.GetHint;

        DisableLifeBubble(hintLifeBubble);
    }

    // re-shuffle
    public void OnShuffleLifeBubbleClick()
    {
        mCurrentAnagram.Shuffle();
        ResetGrid(anagramPanel);
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

    public void OnPointerUp()
    {
        if (AnagramSolved())
        {
            Debug.Log("Solved!");

            ResetGrid(anagramPanel);
            UpdateCurrentAnagram();
        }
        else
        {
            Debug.Log("Not solved");
        }
    }
}
