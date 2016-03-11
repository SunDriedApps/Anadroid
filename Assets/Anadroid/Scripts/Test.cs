using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Test : MonoBehaviour {
    const string FILE_CAPITAL_CITIES = "CapitalCities";
    const int MAX_NUM_OF_HINTS = 1;

    public Text hintText;
    public GameObject shuffledPanel;
    public GameObject solutionPanel;
    public GameObject hintLifeBubble;
    public GameObject shuffleLifeBubble;

    private GridLayoutGroup shuffledGrid;
    private GridLayoutGroup solutionGrid;

    private Text letterText;

    // reference to all letters currently on screen
    private List<GameObject> mLetters;

    private CategoryContainer mCategoryContainer;

    private static Anagram mCurrentAnagram;

    private static bool mTimeUp = false;

    private int mNumOfUsedHints = 0;

    // Use this for initialization
    void Start ()
    {
        shuffledGrid = shuffledPanel.GetComponent<GridLayoutGroup>();
        solutionGrid = solutionPanel.GetComponent<GridLayoutGroup>();

        mLetters = new List<GameObject>();

        mCategoryContainer = CategoryContainer.Load(FILE_CAPITAL_CITIES);

        UpdateCurrentAnagram();
    }

    void Update()
    {
        if(mTimeUp)
        {
            UpdateCurrentAnagram();
            mTimeUp = false;
        }
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

    private bool AnagramSolved()
    {
        if (solutionPanel.transform.childCount < mLetters.Count)
        {
            return false;
        }

        char letter;

        // check every character against the solution
        for (int i = 0; i < solutionPanel.transform.childCount; i++)
        {
            // get letter from game object
            letter = char.Parse(solutionPanel.transform.GetChild(i)
                .GetComponentInChildren<Text>().text);

            // letter doesnt match with solution
            if (letter != mCurrentAnagram.GetSolution[i])
            {
                return false;
            }
        }

        // all letters match with solution
        return true;
    }

    // update the current anagram through the game manager
    public void UpdateCurrentAnagram()
    {
        ResetGrid(solutionPanel);
        ResetGrid(shuffledPanel);

        // get new anagram
        mCurrentAnagram = mCategoryContainer.GetAnagram();

        mCurrentAnagram.Shuffle();

        AddAnagramToShuffleGrid();

        mTimeUp = true;
    }

    private void AddAnagramToShuffleGrid()
    {
        // loop through each character and make a letter prefab
        for (int i = 0; i < mCurrentAnagram.Length; i++)
        {
            // instantiate a letter prefab from the resources folder
            GameObject letter = (GameObject)Instantiate(Resources.Load("AnagramLetter"));

            // set text component of letter
            letterText = letter.GetComponentInChildren<Text>();
            letterText.text = mCurrentAnagram.GetShuffled[i].ToString();

            // add letter to the shuffled grid
            AddToShuffledGrid(letter);

            // add click listener to move letter to solution grid
            letter.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnShuffledLetterClick(letter);
            });

            mLetters.Add(letter);
        }
    }

    private void OnSolutionLetterClick(GameObject letter)
    {
        // insert into shuffled grid
        AddToShuffledGrid(letter);

        // remove previous listener
        letter.GetComponent<Button>().onClick.RemoveAllListeners();

        // letter is now part of shuffled grid; add new click listener
        letter.GetComponent<Button>().onClick.AddListener(() =>
        {
            OnShuffledLetterClick(letter);
        });
    }

    private void OnShuffledLetterClick(GameObject letter)
    {
        // insert in solution grid
        AddToSolutionGrid(letter);

        // remove previous listener
        letter.GetComponent<Button>().onClick.RemoveAllListeners();

        // letter is now part of solution grid; add new click listener
        letter.GetComponent<Button>().onClick.AddListener(() =>
        {
            OnSolutionLetterClick(letter);
        });

        // check if we've had a guess at the solution
        if (solutionPanel.transform.childCount == mLetters.Count)
        {
            if(AnagramSolved())
            {
                Debug.Log("Solved!");
            }
            else
            {
                Debug.Log("Incorrect!");
            }
        }
    }

    // add letter to solution grid panel
    private void AddToSolutionGrid(GameObject letter)
    {
        letter.transform.SetParent(solutionGrid.transform, false);
        letter.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        letter.transform.localPosition = Vector3.zero;
    }

    // add letter to shuffled grid panel
    private void AddToShuffledGrid(GameObject letter)
    {
        letter.transform.SetParent(shuffledGrid.transform, false);
       // letter.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
       // letter.transform.localPosition = Vector3.zero;
    }

    // destroy letters in grid
    private void ResetGrid(GameObject gridPanel)
    {
        foreach (GameObject letter in mLetters)
        {
            Destroy(letter);
        }
    }

    public static void TimeUp()
    {
        mTimeUp = true;
    }

    public void OnHintLifeBubbleClick()
    {
        Debug.Log("Hint clicked");

        mNumOfUsedHints++;

        hintText.text = mCurrentAnagram.GetHint;

        DisableLifeBubble(hintLifeBubble);
    }

    // re-shuffle
    public void OnShuffleLifeBubbleClick()
    {
        mCurrentAnagram.Shuffle();
        ResetGrid(shuffledPanel);
        AddAnagramToShuffleGrid();
    }
}
