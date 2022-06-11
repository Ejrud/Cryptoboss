using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    [SerializeField] private float deadZone = 1f;
    [SerializeField] private bool active = true;

    [Header("Links")]
    public Vector3 CardDisplacement;
    [SerializeField] private CardManager cardManager; 
    [SerializeField] private RectTransform rectTransform;
    private CardParameters parameters;
    private Transform selfTransform;
    private Vector2 offset;
    private float zPosition;
    private bool acceptMove = false;
    private bool hold = false;

    private void Start()
    {
        selfTransform = GetComponent<Transform>();
        parameters = GetComponent<CardParameters>();
    }

    private void Update()
    {
        if (hold && acceptMove)
        {
            // Нахождение позиции курсора
            Vector3 pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 resPos2D = new Vector2(pointerPos.x, pointerPos.y) - offset;
            Vector3 resPosition = new Vector3(resPos2D.x, resPos2D.y, 0);

            // Движение объекта за курсором
            rectTransform.position = new Vector3(resPosition.x, resPosition.y, 100); // 100 - стартовое значение plane distance
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hold = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (active)
        {
            Vector2 pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            offset = pointerPos - new Vector2(selfTransform.position.x, selfTransform.position.y);
            acceptMove = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector3 pos = rectTransform.position;

        acceptMove = false;

        if (pos.x > CardDisplacement.x + deadZone || pos.x < CardDisplacement.x - deadZone || pos.y > CardDisplacement.y + deadZone || pos.y < CardDisplacement.y - deadZone)
        {
            if (!parameters.Selected) 
            {
                cardManager.OnCardSelect(rectTransform);
            }

            parameters.Selected = true;
        }
        else
        {
            // Возврат в изначальное положение
            ReturnCard();
        }
    }

    public void ReturnCard()
    {
        StartCoroutine(SetStartPosition());
    }

    // Если карта не передвинулась с места то вернуть ее в изначальное место
    private IEnumerator SetStartPosition()
    {
        Vector3 startPos = rectTransform.transform.position;
        Vector3 endPos = CardDisplacement;
        Vector3 BezierPos = new Vector3(CardDisplacement.x, rectTransform.position.y, rectTransform.transform.position.z);

        bool move = true;
        float progress = 0f;

        while (move)
        {
            Vector3 p1 = Vector3.Lerp(startPos, BezierPos, progress);
            Vector3 p2 = Vector3.Lerp(BezierPos, endPos, progress);
            Vector3 position = Vector3.Lerp(p1, p2, progress);

            rectTransform.transform.position = position;
            progress += Time.deltaTime + 0.02f;

            if (progress >= 1)
            {
                rectTransform.transform.position = endPos;
                move = false;
            }

            yield return new WaitForUpdate();
        }

        parameters.Selected = false;
        yield return null;
    }
}
