using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSecondPlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCardsDeck;
    [SerializeField] private GameObject rivalCardsDeck;

    [SerializeField] private CardParameters[] friendPlayerCards;
    [SerializeField] private CardParameters[] friendRivalCards;

    private GameObject[] playerFriendCardsObj;
    private GameObject[] rivalFriendCardsObj;

    public void Init(bool twoByTwo)
    {
        playerFriendCardsObj = new GameObject[friendPlayerCards.Length];
        rivalFriendCardsObj = new GameObject[friendRivalCards.Length];

        for (int i = 0; i < friendPlayerCards.Length; i++)
        {
            playerFriendCardsObj[i] = friendPlayerCards[i].gameObject;
            playerFriendCardsObj[i].SetActive(twoByTwo);

            rivalFriendCardsObj[i] = friendRivalCards[i].gameObject;
            rivalFriendCardsObj[i].SetActive(twoByTwo);
        }
    }

    public void HideCards(bool active, bool player)
    {
        if (player)
        {
            for (int i = 0; i < friendPlayerCards.Length; i++)
            {
                playerFriendCardsObj[i].SetActive(active);
            }

            playerCardsDeck.SetActive(!active);
        }
        else
        {
            for (int i = 0; i < friendRivalCards.Length; i++)
            {
                rivalFriendCardsObj[i].SetActive(active);
            }
            playerCardsDeck.SetActive(!active);
        }
        
    }

    public void UpdateRivalCards(Transform[] positions, bool friendTurn, bool audit = false) // Обновляет содержимое карт и смещает на необходимые позиции
    {   
        HideCards(friendTurn, false);

        if (!friendTurn) return;

        int selectedCount = 0;

        for (int i = 0; i < friendRivalCards.Length; i++)
        {
            if (friendRivalCards[i].Selected)
            {
                for (int j = i; j < friendRivalCards.Length; j++)
                {
                    if (j + 1 == friendRivalCards.Length) continue;

                    friendRivalCards[j + 1].GetComponent<CardHandler>().CardDisplacement = positions[j - selectedCount].position;
                    friendRivalCards[j + 1].GetComponent<CardHandler>().IndexPosition = j - selectedCount;
                }
                selectedCount++;
            }
        }

        for (int i = 0; i < friendRivalCards.Length; i++)
        {
            if (!friendRivalCards[i].Selected)
            {
                friendRivalCards[i].GetComponent<CardHandler>().ReturnCard();
            }
        }
    }

    public void UpdatePlayerCards(Transform[] positions, CardData[] parameters, bool myTurn, bool friendTurn) // Обновляет содержимое карт и смещает на необходимые позиции
    {
        HideCards(friendTurn, true); // Скрыть карты союзника если ход игрока

        if (!friendTurn) return;

        Debug.Log("Update friend cards");
        int selectedCount = 0;

        for (int i = 0; i < friendPlayerCards.Length; i++)
        {   
            friendPlayerCards[i].SetCardEffects(parameters[i], i);

            if (friendPlayerCards[i].Selected)
            {
                for (int j = i; j < friendPlayerCards.Length; j++)
                {
                    if (j + 1 == friendPlayerCards.Length) continue;

                    friendPlayerCards[j + 1].GetComponent<CardHandler>().CardDisplacement = positions[j - selectedCount].position;
                    friendPlayerCards[j + 1].GetComponent<CardHandler>().IndexPosition = j - selectedCount;
                }
                selectedCount++;
            }
        }

        for (int i = 0; i < friendPlayerCards.Length; i++)
        {
            if (!friendPlayerCards[i].Selected)
            {
                friendPlayerCards[i].GetComponent<CardHandler>().ReturnCard();
            }
        }
    }
}
