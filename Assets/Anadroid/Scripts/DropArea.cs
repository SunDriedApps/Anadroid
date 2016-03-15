using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class DropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
  
    public void OnPointerEnter(PointerEventData eventData)
    {
      //  Debug.Log("Pointer Enter");
        if (eventData.pointerDrag == null)
            return;

        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
        if (d != null)
        {
            d.letterGapParent = this.transform;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       //Debug.Log("Pointer Exit");
        if (eventData.pointerDrag == null)
            return;

        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
        if (d != null && d.letterGapParent == this.transform)
        {
            d.letterGapParent = d.originalParent;
        }

    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerDrag.name + " was dropped on " + gameObject.name);


        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
        if (d != null)
        {
            d.originalParent = this.transform;
        }
    }

}
