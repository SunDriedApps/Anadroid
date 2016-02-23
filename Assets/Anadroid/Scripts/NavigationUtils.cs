using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class NavigationUtils : MonoBehaviour {

	public static void ChangeScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
