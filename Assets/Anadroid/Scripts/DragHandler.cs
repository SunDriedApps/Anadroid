using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    public static GameObject itemBeingDragged;
    Vector3 startPosition;
    Transform startParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("onBeginDrag accesed");

        itemBeingDragged = gameObject;
        startPosition = transform.position;
        startParent = transform.parent;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Need to determine which works better!
        // transform.position = Input.GetTouch(0).position;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        // transform.parent will be still equal to startParent if itembeingdragged is null (drophandler onDrop not used)
        if (transform.parent == startParent)
        {
            Debug.Log("parent transform = to start ");

            transform.position = startPosition;
        }
    }
}
