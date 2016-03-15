using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform originalParent = null;
    public Transform letterGapParent = null;

    GameObject letterGap = null;

    private LetterOnEndDrag mLetterOnEndDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        letterGap = new GameObject();
        letterGap.name = "LetterGap";
        letterGap.transform.SetParent(this.transform.parent);

        LayoutElement le = letterGap.AddComponent<LayoutElement>();
        le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        letterGap.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

        originalParent = this.transform.parent;
        letterGapParent = originalParent;
        this.transform.SetParent(this.transform.parent.parent);

        GetComponent<CanvasGroup>().blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;

        if (letterGap.transform.parent != letterGapParent)
            letterGap.transform.SetParent(letterGapParent);

        int newSiblingIndex = letterGapParent.childCount;

        for (int i = 0; i < letterGapParent.childCount; i++)
        {
            if (this.transform.position.x  < letterGapParent.GetChild(i).position.x)
            {


                newSiblingIndex = i ;

                if (letterGap.transform.GetSiblingIndex() < newSiblingIndex )
                {
                    newSiblingIndex--;
                }
                break;
            }
        }

        letterGap.transform.SetSiblingIndex(newSiblingIndex); 
    }

    

    public void SetOnEndDrag(LetterOnEndDrag p)
    {
        mLetterOnEndDrag = p;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End drag called");

        this.transform.SetParent(originalParent);
        this.transform.SetSiblingIndex(letterGap.transform.GetSiblingIndex());

        GetComponent<CanvasGroup>().blocksRaycasts = true;

        Destroy(letterGap); 

        mLetterOnEndDrag.OnEndDrag();
    }
}
