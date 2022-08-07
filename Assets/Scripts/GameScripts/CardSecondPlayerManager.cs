using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSecondPlayerManager : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private GameObject[] playerCards;
    [SerializeField] private GameObject[] rivalCards;

    [Header("StackPositions")]
    [SerializeField] private Transform[] playerStackCardPositions;
    [SerializeField] private Transform[] rivalStackCardPositions;

    [Header("Save data")]
    [SerializeField] private CardData[] playerCardDatas = new CardData[5]; 
    private CardData[] rivalCardDatas;

    public void SwipePlayerCards(CardData[] cardData, bool player) // Передавать карты соперника и карты текущие карты игрока 
    {
        for (int i = 0; i < 5; i++)
        {
            playerCardDatas[i] = cardData[i];

            if (cardData[i].Used)
            {
                playerCards[i].GetComponent<CardParameters>().Selected = true;
                // playerCards[i].transform.position = playerStackPosition.position;
                playerCards[i].SetActive(false);
            }
            else
            {
                playerCards[i].SetActive(true);
                playerCards[i].transform.position = playerStackCardPositions[i].position;

                CardParameters cardParameters = playerCards[i].GetComponent<CardParameters>();
                cardParameters.SetCardEffects(cardData[i], i);
                cardParameters.Selected = false;
            }
        }
    }

    public void SetCurrentPlayerCards()
    {

    }
}
