using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(NetworkView))]

public class DropHandler : MonoBehaviour, IDropHandler {
    public GameObject letter 
    {
        get 
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }
            return null; 
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!letter)
        {
            DragHandler.itemBeingDragged.transform.SetParent(transform);
        }
    }
}
