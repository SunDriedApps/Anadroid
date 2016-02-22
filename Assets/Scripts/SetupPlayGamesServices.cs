using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;

public class SetupPlayGamesServices : MonoBehaviour
{
    // Blah blah
    void Start()
    {
        // Select the Google Play Games platform as our social platform implementation
        GooglePlayGames.PlayGamesPlatform.Activate();

        Social.localUser.Authenticate((bool success) => {

            if (success)
            {

                string token = GooglePlayGames.PlayGamesPlatform.Instance.GetToken();
                Debug.Log(token);
            }
            else {

            }
        });
    }
}
