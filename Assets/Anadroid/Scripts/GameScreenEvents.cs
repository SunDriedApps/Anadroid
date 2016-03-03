using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameScreenEvents : MonoBehaviour {

    public Text score;
    public Text opponentScore;
    public Text solutionText;
    public Text shuffledText;

    private Anagram mCurrentAnagram;

    public void Start()
    {
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
            case GameManager.GameState.Aborted:
                GameManager.Instance.CleanUp();
                NavigationUtils.ShowMainMenu();
                break;

            case GameManager.GameState.Finished:
                if(GameManager.Instance.GetScore > GameManager.Instance.GetOpponentScore)
                {
                    solutionText.text = "You won!";
                }
                else
                {
                    solutionText.text = "You suck!";
                }
                break;
        }

        // check if we've moved onto a new anagram
        if (!mCurrentAnagram.Equals(GameManager.Instance.GetCurrentAnagram)) {
            UpdateCurrentAnagram();
        }

        // update scores
        score.text = GameManager.Instance.GetScore.ToString();
        opponentScore.text = GameManager.Instance.GetOpponentScore.ToString();
    }

    public void IncrementScore()
    {
        GameManager.Instance.IncrementScore();
    }

    // update the current anagram
    private void UpdateCurrentAnagram()
    {
        mCurrentAnagram = GameManager.Instance.GetCurrentAnagram;

        // set text fields
        solutionText.text = mCurrentAnagram.GetSolution;
        shuffledText.text = mCurrentAnagram.GetShuffled;
    }
}
