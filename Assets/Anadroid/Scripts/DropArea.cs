using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class DropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("On drop");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
       // Debug.Log("Pointer Enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       // Debug.Log("Pointer Exit");
    }
}
