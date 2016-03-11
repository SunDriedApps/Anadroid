using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/**
 * A class to control the timer on the game screen which specifies
 * the amount of time a player will have to solve each anagram 
**/

public class TestTimer : MonoBehaviour
{

    // 20 seconds to solve each anagram
    private const float TIME_TO_SOLVE = 20.0f;

    public Image timeBar;

    private static float sTimeRemaining;

    void Start()
    {
        sTimeRemaining = TIME_TO_SOLVE;

        UpdateTime();
    }

    void Update()
    {

        // time up; move onto next anagram
        if (sTimeRemaining <= 0.0)
        {
            Test.TimeUp();
            ResetTime();
        }

        UpdateTime();
    }

    // decrement time and update text
    private void UpdateTime()
    {
        sTimeRemaining -= Time.deltaTime;

        // update time display
        timeBar.fillAmount = sTimeRemaining / TIME_TO_SOLVE;
    }

    public static void ResetTime()
    {
        sTimeRemaining = TIME_TO_SOLVE;
    }
}
