using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    Transform originalParent = null;

    GameObject letterGap = null;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("onBeginDrag");

        letterGap = new GameObject();
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
       // Debug.Log("onDrag");
        this.transform.position = eventData.position;

        int newSiblingIndex =  this.transform.GetSiblingIndex();//letterGap.transform.GetSiblingIndex();
        //Debug.Log("sibling  index: " + newSiblingIndex);

        for (int i = 0; i < originalParent.childCount; i++)
        {
            if (this.transform.position.x  < originalParent.GetChild(i).position.x)
            {
               Debug.Log("this.transform.position.x : " + this.transform.position.x);
               Debug.Log("originalParent.GetChild(i).position.x : " + originalParent.GetChild(i).position.x);


                newSiblingIndex = i ;
                //Debug.Log("new sibling index" + i );

               // Debug.Log("letterGap.transform.GetSiblingIndex()" + letterGap.transform.GetSiblingIndex());

                if (letterGap.transform.GetSiblingIndex() < newSiblingIndex )
                {
                    newSiblingIndex--;
                }
                break;
            }
            /*else if (this.transform.position.x > originalParent.GetChild( i).position.x)
            {
                newSiblingIndex = i;
                break;
            }*/
        }

        letterGap.transform.SetSiblingIndex(newSiblingIndex); 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("onEndDrag");

        this.transform.SetParent(originalParent);
        this.transform.SetSiblingIndex(letterGap.transform.GetSiblingIndex());

        GetComponent<CanvasGroup>().blocksRaycasts = true;

        Destroy(letterGap); 

    }
}
