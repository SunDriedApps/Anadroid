using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameScreenEvents : MonoBehaviour {

    private bool checkButtonOn = false;
    private bool buttonMoveFin = false;
    GameObject buttonToMove = null;
   // GameObject nextBlank = null;

    /*public void getNextBlank()
    {
        nextBlank
    }*/

    // this method is called when by on click button, the button supplies it's game object 
    // boolean checkForButton is switched, buttonToMove is set
    public void buttonPressed(GameObject buttonClicked)
    {
        checkButtonOn = true;
        buttonMoveFin = false;
        buttonToMove = buttonClicked;
    }

    public void Update()
    {
        if (checkButtonOn)
        {
              MoveButton(buttonToMove/*,nextBlankButton8*/);
        }
        
    }

    public void MoveButton(GameObject button/*, GameObject buttonDest*/)
    {
        float speed = 3500f;

        Vector3 newPos = new Vector3(150, 600, 0);
      
        float step = speed * Time.deltaTime;
        button.transform.position = Vector3.MoveTowards(button.transform.position, newPos, step);

        

       
    }
}
