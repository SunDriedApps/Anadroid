using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(NetworkView))]

public class DynamicLetterSlotDrop : MonoBehaviour, IDropHandler
{
    // create letter object refernecing the the destination objects potential child 


    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("onDrop solutionPanel  accesed");


        //DragHandler.itemBeingDragged.transform.SetParent(transform.GetSiblingIndex());
        //Test2.solutionPanelCheck();
    }
}

