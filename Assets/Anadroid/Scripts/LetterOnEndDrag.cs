using UnityEngine;
using System.Collections;

/*
** This interface was specifically made to be implemented in GameScreenEvents
** so we can handle the end of a letter drag event directly after LetterBehaviour
** calls it's OnEndDrag
*/

public interface LetterOnEndDrag {
    void OnEndDrag();
}
