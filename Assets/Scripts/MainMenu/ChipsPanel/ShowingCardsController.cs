using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowingCardsController : MonoBehaviour
{
    [SerializeField] private User user;

    [Header("Cards")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private CardParameters[] cardParameters; // �������� ����. ������ ������ ����� (�����, �������, ��������)

    private void Start()
    {
        GlobalEventManager.OnSelectCards.AddListener(UpdateWindow);         // ���� ����� ������ �� �����, �� ��������� ���� � �������

        PrepareCards();
    }

    private void UpdateWindow(CardData[] cardDatas)                         // ChipFrameData �������� "������������" ���������.
    {
        PutAwayCards();
        ShowCards(cardDatas.Length);

        for (int i = 0; i < cardDatas.Length; i++)                                        
        {
            cardParameters[i].SetCardEffects(cardDatas[i], i);
        } 
    }

    private void PrepareCards()                                             // ������ �����
    {
        int count = 0;
        for (int i = 0; i < user.ChipParam.Count; i++)
        {
            count += user.ChipParam[i].CardDeck.Length;
        }

        cardParameters = new CardParameters[count];

        for (int i = 0; i < cardParameters.Length; i++)
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

    private void ShowCards(int cardCount)                                                // �������� �����
    {
        for (int i = 0; i < cardCount; i++)
        {
            cardParameters[i].gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        GlobalEventManager.OnSelectCards.RemoveListener(UpdateWindow);
    }

    private void OnDisable()                                                // ���� ���� � ������� �����������, �� ����� ��������
    {
        PutAwayCards();
    }
}
