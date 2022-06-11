using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowingCardsController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _cardFrame;

    [Header("Cards")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private CardParameters[] cardParameters = new CardParameters[10]; // Оболочка карт. Хранит данные карты (атаку, оборону, описание)


    private void Start()
    {
        GlobalEventManager.OnSelectCards.AddListener(UpdateWindow);         // Если игрок нажмет на фишку, то обновится окно с картами

        PrepareCards();
    }

    private void UpdateWindow(CardData[] cardDatas)                         // ChipFrameData являются "наблюдаемыми" объектами.
    {
        if (!cardParameters[0].gameObject.activeInHierarchy)
        { 
            ShowCards();
        }

        for (int i = 0; i < 10; i++)                                        // у каждой фишки 10 карт
        {
            cardParameters[i].SetCardEffects(cardDatas[i], i);
        } 
    }

    private void PrepareCards()                                             // Убрать карты
    {
        cardParameters = new CardParameters[10];

        for (int i = 0; i < 10; i++)
        { 
            Transform cardTransform = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity).GetComponent<Transform>();
            cardTransform.SetParent(contentContainer);
            cardTransform.localScale = new Vector3(1, 1, 1);
            cardParameters[i] = cardTransform.GetComponent<CardParameters>();
            cardParameters[i].Session = false;
        }

        PutAwayCards();
    }

    private void PutAwayCards()
    {
        foreach (CardParameters card in cardParameters)
        {
            card.gameObject.SetActive(false);
        }
    }

    private void ShowCards()                                                // Показать карты
    {
        foreach (CardParameters card in cardParameters)
        {
            card.gameObject.SetActive(true); // card
        }
    }

    private void OnDestroy()
    {
        GlobalEventManager.OnSelectCards.RemoveListener(UpdateWindow);
    }

    private void OnDisable()                                                // Если окно с фишками закрывается, то карты прячутся
    {
        PutAwayCards();
    }
}
