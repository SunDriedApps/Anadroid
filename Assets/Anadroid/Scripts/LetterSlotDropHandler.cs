using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(NetworkView))]

public class LetterSlotDropHandler : MonoBehaviour, IDropHandler {
    // create letter object refernecing the the destination objects potential child 
    public GameObject letter 
    {
        get 
        {
            // check that destination transform has children 
            if (transform.childCount > 0)
            {
                Debug.Log("has child");
                return transform.GetChild(0).gameObject;
            }
            Debug.Log("has  no child");

            return null; 
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("onDrop accesed");

        
        if (letter)  //if slot occupied, swap letters 
        {
           //   GameObject occupyingLetter = transform.GetChild(0).gameObject;
           // occupyingLetter.transform.SetParent(DragHandler.itemBeingDragged.transform.parent);


            //occupyingLetter.transform.SetSiblingIndex(occupyingLetter.transform.GetSiblingIndex()+1);

        }
        // set letter to new slot
        DragHandler.itemBeingDragged.transform.SetParent(transform);


    }
}
