using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour {

    public Text scoreText;
    public Text opponentScoreText;
    public AudioSource anagramSolvedAudio;
    public AudioSource opponentSolvedAudio;

    private int mScore;
    private int mOpponentScore;

    // Use this for initialization
    void Start () {

        mScore = GameManager.Instance.Score;

        mOpponentScore = GameManager.Instance.OpponentScore;
	}
	
	// Update is called once per frame
	void Update () {

        // we've scored
        if(mScore != GameManager.Instance.Score)
        {
            mScore = GameManager.Instance.Score;

            scoreText.text = mScore.ToString();
            scoreText.color = Color.green;

            StartCoroutine(ResetScoreColor(scoreText));

            if (GameManager.Instance.SoundEffectsEnabled)
            {
                anagramSolvedAudio.Play();
            }
        }

        // opponents has scored
        if (mOpponentScore != GameManager.Instance.OpponentScore)
        {
            mOpponentScore = GameManager.Instance.OpponentScore;

            opponentScoreText.text = mOpponentScore.ToString();
            opponentScoreText.color = GameManager.Instance.CustomRed;

            StartCoroutine(ResetScoreColor(opponentScoreText));

            if(GameManager.Instance.SoundEffectsEnabled)
            {
                opponentSolvedAudio.Play();
            }
        }
    }

    // reset color after anagram transition time
    IEnumerator ResetScoreColor(Text score) 
    {
        yield return new WaitForSeconds(GameManager.ANAGRAM_TRANSITION_TIME);

        score.color = Color.white;
    }
}
