using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/*
** A helper class to change scenes in the game
*/

public class NavigationUtils : MonoBehaviour {

    public static void ShowMainMenu()
    {
        ChangeScene(0);
    }

    public static void ShowGameScreen()
    {
        ChangeScene(1);
    }

	private static void ChangeScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
