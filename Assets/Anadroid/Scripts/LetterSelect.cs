using UnityEngine;
using System.Collections;


public class LetterSelect : MonoBehaviour  {
    
	
	public void MoveButton (GameObject button) {
	    float speed = 10;

        Vector3 newPos = new Vector3(0, 0, 0);

        Vector3 currentPos = button.transform.position;

        Vector3 directionOfMove = newPos - currentPos;

        directionOfMove.Normalize();

        button.transform.Translate(
            (directionOfMove.x * speed * Time.deltaTime),
            (directionOfMove.y * speed * Time.deltaTime),
            (directionOfMove.z * speed * Time.deltaTime)
            );

	}
}
