using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameScreenEvents : MonoBehaviour {

    public void OnSendMessage()
    {
        int rnd = GetRandomNum();

        GameManager.Instance.SendMessage(rnd);

        MessageDisplayManager.SetMessageSent(rnd.ToString());
    }

    private int GetRandomNum()
    {
        int randomNum = Random.Range(0, 10);

        Debug.Log("Random = " + randomNum);

        return randomNum;
    }
}
