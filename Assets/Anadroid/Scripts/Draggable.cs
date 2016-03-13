using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerUpHandler
{
    Transform originalParent = null;

    GameObject letterGap = null;

    private LetterOnPointerUp mLetterOnPointerUp;

    public void OnBeginDrag(PointerEventData eventData)
    {
        letterGap = new GameObject();
        letterGap.name = "LetterGap";
        letterGap.transform.SetParent(this.transform.parent);

        LayoutElement le = letterGap.AddComponent<LayoutElement>();
        le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
        le.flexibleWidth = this.GetComponent<LayoutElement>().flexibleWidth;
        le.flexibleHeight = this.GetComponent<LayoutElement>().flexibleHeight;

        letterGap.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

        originalParent = this.transform.parent;
        this.transform.SetParent(this.transform.parent.parent);

        GetComponent<CanvasGroup>().blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;

        int newSiblingIndex =  this.transform.GetSiblingIndex();

        for (int i = 0; i < originalParent.childCount; i++)
        {
            if (this.transform.position.x  < originalParent.GetChild(i).position.x)
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

    public void OnPointerUp(PointerEventData eventData)
    {
        this.transform.SetParent(originalParent);
        this.transform.SetSiblingIndex(letterGap.transform.GetSiblingIndex());

        GetComponent<CanvasGroup>().blocksRaycasts = true;

        Destroy(letterGap);

        mLetterOnPointerUp.OnPointerUp();
    }

    public void SetOnPointerUp(LetterOnPointerUp p)
    {
        mLetterOnPointerUp = p;
    }
}
