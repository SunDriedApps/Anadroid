using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerController : MonoBehaviour {

    const float TOTAL_TIME_TO_SOLVE_ANAGRAM = 40f;
    const float TIME_ALMOST_UP_WARNING = 5f;

    public Text timerText;
    public AudioSource timeAlmostUpAudio;
    public AudioSource timeUpAudio;

    public static bool sTimerEnabled = true;

    // time remaining to solve the current anagram
    private static float sTimeRemaining;

    // is time up on the current anagram
    private bool mTimeUp = false;
    private bool mTimeAlmostUp = false;

    private Anagram mCurrentAnagram;


    void Start ()
    {
        sTimeRemaining = TOTAL_TIME_TO_SOLVE_ANAGRAM;

        mCurrentAnagram = GameManager.Instance.CurrentAnagram;
    }
	
	void Update ()
    {
        if(!sTimerEnabled)
        {
            return;
        }

        if(mCurrentAnagram == null)
        {
            mCurrentAnagram = GameManager.Instance.CurrentAnagram;
            return;
        }

        // time up
        if (sTimeRemaining <= 0 && !mTimeUp)
        {
            Debug.Log("Time up");

            if (GameManager.Instance.SoundEffectsEnabled && !mTimeUp)
            {
                timeUpAudio.Play();
            }

            mTimeUp = true;

            GameManager.Instance.GetNextAnagram();
        }

        // check if we've moved onto a new anagram
        if(!mCurrentAnagram.Equals(GameManager.Instance.CurrentAnagram))
        {
            sTimeRemaining = TOTAL_TIME_TO_SOLVE_ANAGRAM;

            timerText.color = Color.white;

            mCurrentAnagram = GameManager.Instance.CurrentAnagram;

            mTimeUp = false;

            mTimeAlmostUp = false;
        }

        // play time almost up sound
        if(sTimeRemaining <= TIME_ALMOST_UP_WARNING)
        {
            if(GameManager.Instance.SoundEffectsEnabled && !mTimeAlmostUp)
            {
                timeAlmostUpAudio.Play();

                mTimeAlmostUp = true;

                timerText.color = GameManager.Instance.CustomRed;
            }
        }

        // update timer
        if(GameManager.Instance.State == GameManager.GameState.Playing)
        {
            sTimeRemaining -= Time.deltaTime;

            timerText.text = sTimeRemaining.ToString("0");
        }
    }

    public static float TimeRemaining
    {
        get
        {
            return sTimeRemaining;
        }
    }

    public static void EnableTimer()
    {
        sTimerEnabled = true;
    }

    public static void DisableTimer()
    {
        sTimerEnabled = false;
    }
}
