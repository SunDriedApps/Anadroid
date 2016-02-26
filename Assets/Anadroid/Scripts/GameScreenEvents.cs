using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameScreenEvents : MonoBehaviour {

    const string MESSAGE_SENT = "MessageSent";

    public void OnSendMessage()
    {
        int rnd = GetRandomNum();

        GameManager.Instance.SendMessage(rnd);

        GameObject messageSent = GameObject.Find(MESSAGE_SENT);
        Text t = messageSent.GetComponent<Text>();
        t.text = rnd.ToString();
    }

    private int GetRandomNum()
    {
        int randomNum = Random.Range(0, 10);

        Debug.Log("Random = " + randomNum);

        return randomNum;
    }
}
