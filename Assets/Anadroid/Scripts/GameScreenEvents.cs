using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameScreenEvents : MonoBehaviour {

    const string GAME_OVER_DIALOG_MESSAGE = "GameOverMessage";
    const string MESSAGE_WINNER = "YOU WIN";
    const string MESSAGE_LOSER = "YOU LOSE";
    const int MAX_NUM_OF_HINTS = 1;

    // game objects and components
    public Text score;
    public Text opponentScore;
    public GameObject timer;
    public Text hintText;
    public GameObject solutionPanel;
    public GameObject shuffledPanel;
    public GameObject dialogPanel;
    public GameObject gameOverDialog;
    public GameObject opponentDisconnectedDialog;
    private GridLayoutGroup solutionGrid;
    private GridLayoutGroup shuffledGrid;
    public GameObject shuffleLifeBubble;
    public GameObject hintLifeBubble;
    public GameObject revealLifeBubble;

    
    // used to get reference to every letter's text component
    private Text letterText;

    // reference to all letters currently on screen
    private List<GameObject> mLetters;

    // the current anagram being solved
    private Anagram mCurrentAnagram;

    // the number of hints used in the 
    private int mNumOfUsedHints = 0;

    // only 1 re-shuffle allowed per anagram
    private bool mUsedShuffle = false;


    public void Start()
    {
        
        if(GameManager.Instance == null)
        {
            return;
        }

        mLetters = new List<GameObject>();

        // get grid layouts from panels
        solutionGrid = solutionPanel.GetComponent<GridLayoutGroup>();
        shuffledGrid = shuffledPanel.GetComponent<GridLayoutGroup>();

        // attempt to get the first anagram
        UpdateCurrentAnagram();
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
                timer.SetActive(false);
                //gameOverDialog.SetActive(true);
                break;

            case GameManager.GameState.Aborted:
                timer.SetActive(false);
                dialogPanel.SetActive(true);
                opponentDisconnectedDialog.SetActive(true);
                break;
        }

        // check if we've moved onto a new anagram
        if (!mCurrentAnagram.Equals(GameManager.Instance.GetCurrentAnagram)) {
            
            // reset both grids
            ResetGrid(solutionPanel);
            ResetGrid(shuffledPanel);

            // clear hint text
            hintText.text = "";

            UpdateCurrentAnagram();

            // renable hint life bubble if we haven't reached maximum
            if (mNumOfUsedHints != MAX_NUM_OF_HINTS)
            {
                EnableLifeBubble(hintLifeBubble);
            }

            if(mUsedShuffle)
            {
                mUsedShuffle = false;
                EnableLifeBubble(shuffleLifeBubble);
            }
        }

        // update opponent score
        opponentScore.text = GameManager.Instance.GetOpponentScore.ToString();
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
    private void UpdateCurrentAnagram()
    {
        // get new anagram
        mCurrentAnagram = GameManager.Instance.GetCurrentAnagram;

        if(mCurrentAnagram == null)
        {
            return;
        }

        AddAnagramToShuffleGrid();
    }

    // handle the click event of a letter in the solution panel
    private void OnSolutionLetterClick(GameObject letter)
    {
        // insert into shuffled grid
        AddToShuffledGrid(letter);

        // remove previous listener
        letter.GetComponent<Button>().onClick.RemoveAllListeners();

        // add click listener to move letter to shuffled grid
        letter.GetComponent<Button>().onClick.AddListener(() =>
        {
            OnShuffledLetterClick(letter);
        });
    }

    // handle a click event of a letter in the shuffle panel
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
                // increment score
                GameManager.Instance.IncrementScore();

                // send update to opponent
                GameManager.Instance.SendScoreUpdate();

                // update onscreen score
                score.text = GameManager.Instance.GetScore.ToString();

                // reset anagram timer
                Timer.ResetTime();

                Debug.Log("Anagram Solved!");
            }
            else
            {
                Debug.Log("Incorrect guess!");
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
        letter.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        letter.transform.localPosition = Vector3.zero;
    }

    // destroy letters in grid
    private void ResetGrid(GameObject gridPanel)
    {
        foreach (GameObject letter in mLetters)
        {
            Destroy(letter);
        }

        mLetters.Clear();
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

    // re-shuffle
    public void OnShuffleLifeBubbleClick()
    {
        mUsedShuffle = true;

        mCurrentAnagram.Shuffle();

        ResetGrid(shuffledPanel);

        AddAnagramToShuffleGrid();

        DisableLifeBubble(shuffleLifeBubble);
    }

    // show a hint for the current anagram
    public void OnHintLifeBubbleClick()
    {
        mNumOfUsedHints++;

        hintText.text = mCurrentAnagram.GetHint;

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

    public void OnBackToMainScreenClicked()
    {
        NavigationUtils.ShowMainMenu();
    }
}
