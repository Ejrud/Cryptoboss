using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlidingChips : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    public bool AcceptMove;

    public void OnPointerEnter(PointerEventData eventData)
    {
        AcceptMove = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AcceptMove = false;
    }
}
