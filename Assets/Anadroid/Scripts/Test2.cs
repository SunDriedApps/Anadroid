using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Test2 : MonoBehaviour
{

    public GameObject shuffledPanel;
    public GameObject solutionPanel;

    private GridLayoutGroup shuffledGrid;
    private GridLayoutGroup solutionGrid;

    // reference to all letters currently on screen
    private List<GameObject> mLetters;

    private string mSolution = "23014";

    // Use this for initialization
    void Start()
    {
        shuffledGrid = shuffledPanel.GetComponent<GridLayoutGroup>();
        solutionGrid = solutionPanel.GetComponent<GridLayoutGroup>();

        mLetters = new List<GameObject>();

        // add shuffled letters to shuffled grid
        Text t;
        for (int i = 0; i < 5; i++)
        {
            GameObject letter = (GameObject)Instantiate(Resources.Load("AnagramLetter"));

            mLetters.Add(letter);

            t = letter.GetComponentInChildren<Text>();
            t.text = i.ToString();

            letter.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnShuffledLetterClick(letter);
            });

            // insert into shuffled grid
            AddToShuffledGrid(letter);  


        }
    }

    void Update()
    {

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
            letter = char.Parse(solutionPanel.transform.GetChild(i).GetComponentInChildren<Text>().text);

            // letter doesnt match with solution
            if (letter != mSolution[i])
            {
                return false;
            }
        }

        // all letters match with solution
        return true;
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
            if (AnagramSolved())
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
}
