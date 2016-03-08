using UnityEngine;
using System.Collections;

public class Element : MonoBehaviour {

    public bool isEmpty;
    public Sprite emptyTexture;
    public Sprite letter;
    

    // Use this for initialization
    void Start () {
        loadTexture();
    }

    public void loadTexture()
    {
        if (isEmpty)
            GetComponent<SpriteRenderer>().sprite = emptyTexture;
        else
            GetComponent<SpriteRenderer>().sprite = letter;
    }


}
