using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageDisplayManager : MonoBehaviour {

    private static MessageDisplayManager sInstance;

    public GameObject messageRecieved;
    public GameObject messageSent;

    void Start()
    {
        sInstance = new MessageDisplayManager();
    }

    public static void SetMessageRecieved(string status)
    {
        Text text = sInstance.GetComponent<Text>();
        text.text = status;
    }

    public static void SetMessageSent(string message)
    {
        Text text = sInstance.GetComponent<Text>();
        text.text = message;
    }

    public static MessageDisplayManager Instance
    {
        get
        {
            return sInstance;       
        }
    }
}
