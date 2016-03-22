using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System;

/*
** a class to handle the drag behaviour of a letter tile and the 
** behaviour of it's surrounding tiles
*/

public class LetterBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    const float LETTER_TILE_WIDTH = 140f;
    const string GAME_OBJECT_LETTER_POP = "LetterPopAudioSource";
    const int LETTER_TILE_DRAG_OFFSET = 10;

    private Transform anagramPanelTransform;

    // a temporary game object put in place where a letter tile
    // has been taken out of the grid
    private GameObject letterGap;

    // is the associated letter tile locked?
    private bool mLocked = false;

    // a callback interface used in GameScreenEvents
    private LetterOnEndDrag mLetterOnEndDrag;

    // used to keep track of last x position of drag
    // which will help determine if we're moving left 
    // or right
    private float mLastPosX;

   
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(mLocked)
        {
            return;
        }

        letterGap = new GameObject();
        letterGap.name = "LetterGap";
        letterGap.transform.SetParent(transform.parent);

        LayoutElement le = letterGap.AddComponent<LayoutElement>();
        le.preferredWidth = GetComponent<LayoutElement>().preferredWidth;
        le.preferredHeight = GetComponent<LayoutElement>().preferredHeight;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        letterGap.transform.SetSiblingIndex(transform.GetSiblingIndex());

        anagramPanelTransform = transform.parent;
        transform.SetParent(transform.parent.parent);

        GetComponent<CanvasGroup>().blocksRaycasts = false;

        mLastPosX = eventData.position.x;
    }

    int newLetterGapIndex;
    public void OnDrag(PointerEventData eventData)
    {
        if(mLocked)
        {
            return;
        }

        transform.position = eventData.position;

        newLetterGapIndex = letterGap.transform.GetSiblingIndex();

        // moving left
        if (transform.position.x < mLastPosX)
        {
            Debug.Log("Moving left");
            for (int i = 0; i < anagramPanelTransform.childCount; i++)
            {
                // swap tiles
                if ((transform.position.x - LETTER_TILE_WIDTH / 4) < anagramPanelTransform.GetChild(i).position.x)
                {
                    // if the letter tile is locked skip this iteration
                    if (anagramPanelTransform.GetChild(i).name == "Letter(Clone)")
                    {
                        if (anagramPanelTransform.GetChild(i).GetComponent<LetterBehaviour>().Locked)
                        {
                            continue;
                        }
                    }

                    newLetterGapIndex = i;


                    if (letterGap.transform.GetSiblingIndex() < newLetterGapIndex)
                    {
                        newLetterGapIndex--;
                    }
                    break;
                }
            }
        }
        // moving right
        else if(transform.position.x > mLastPosX)
        {
            Debug.Log("Moving right");

            for (int i = letterGap.transform.GetSiblingIndex() + 1; i < anagramPanelTransform.childCount; i++)
            {
                // check are we far enough to the right to 
                if ((transform.position.x + LETTER_TILE_WIDTH / 4) > anagramPanelTransform.GetChild(i).position.x)
                {
                    // if the letter tile is locked skip this iteration
                    if (anagramPanelTransform.GetChild(i).name == "Letter(Clone)")
                    {
                        if (anagramPanelTransform.GetChild(i).GetComponent<LetterBehaviour>().Locked)
                        {
                            continue;
                        }
                    }

                    newLetterGapIndex = i;

                    if (letterGap.transform.GetSiblingIndex() > newLetterGapIndex)
                    {
                        newLetterGapIndex++;
                    }
                    break;
                }
            }
        }

        // update last x position
        mLastPosX = eventData.position.x;

        // swap letter gap with the letter at position pointed to by newLetterGapIndex
        anagramPanelTransform.GetChild(newLetterGapIndex).SetSiblingIndex(letterGap.transform.GetSiblingIndex());
        letterGap.transform.SetSiblingIndex(newLetterGapIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(mLocked)
        {
            return;
        }

        Debug.Log("End drag called");

        transform.SetParent(anagramPanelTransform);

        // if we've moved
        if(transform.GetSiblingIndex() != letterGap.transform.GetSiblingIndex())
        {
            transform.SetSiblingIndex(letterGap.transform.GetSiblingIndex());
            
            if (GameManager.Instance.SoundEffectsEnabled)
            {
                GameObject.Find(GAME_OBJECT_LETTER_POP).GetComponent<AudioSource>().Play();
            }
        }

        GetComponent<CanvasGroup>().blocksRaycasts = true;

        Destroy(letterGap);

        // OnEndDrag has been implemented in GameScreenEvents
        mLetterOnEndDrag.OnEndDrag();
    }

    private bool LetterTileLocked(int index)
    {
        // if the letter tile is locked return 
        if (anagramPanelTransform.GetChild(index).name == "Letter(Clone)")
        {
            if (anagramPanelTransform.GetChild(index).GetComponent<LetterBehaviour>().Locked)
            {
                return true;
            }
        }

        return false;
    }

    // set the LetterOnDrag interface
    public void SetOnEndDrag(LetterOnEndDrag p)
    {
        mLetterOnEndDrag = p;
    }

    public bool Locked
    {
        get
        {
            return mLocked;
        }
    }

    public void SetLocked()
    {
        mLocked = true;
    }
}
