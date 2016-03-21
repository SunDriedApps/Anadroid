using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnagramCounterController : MonoBehaviour {

    const int MAX_ANAGRAMS_VS = 10;

    public Text anagramCountText;

    private Anagram mCurrentAnagram;
    private int mAnagramCount = 1;
    private int mMaxAnagrams;

    // Use this for initialization
    void Start ()
    {
        switch(GameManager.Instance.GameType)
        {
            case GameManager.GAME_TYPE_VS:
                mMaxAnagrams = MAX_ANAGRAMS_VS;
                break;
        }

        mCurrentAnagram = GameManager.Instance.CurrentAnagram;

        UpdateAnagramCountText();
	}
	
	// Update is called once per frame
	void Update ()
    {

        if(GameManager.Instance.State != GameManager.GameState.Playing)
        {
            return;
        }

        if (mCurrentAnagram == null)
        {
            mCurrentAnagram = GameManager.Instance.CurrentAnagram;
            return;
        }

        // check if we've moved onto a new anagram
        if(!mCurrentAnagram.Equals(GameManager.Instance.CurrentAnagram))
        {
            mAnagramCount++;

            if(mAnagramCount == mMaxAnagrams + 1)
            {
                GameManager.Instance.SetState(GameManager.GameState.Finished);
                return;
            }

            UpdateAnagramCountText();

            mCurrentAnagram = GameManager.Instance.CurrentAnagram;
        }
	}

    private void UpdateAnagramCountText()
    {
        anagramCountText.text = mAnagramCount.ToString() +
            "/" + MAX_ANAGRAMS_VS.ToString();
    }
}
