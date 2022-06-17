using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Cinemachine;

public class CardManager : NetworkBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CardParameters[] playerCards;
    [SerializeField] private CardParameters[] rivalCards;

    [Header("Card transition setings")]
    [SerializeField] private Transform[] playerCardPositions;
    [SerializeField] private Transform[] rivalCardPositions;

    [Header("Card table")]
    [SerializeField] private Transform tablePosition;
    [SerializeField] private Transform playerCardTrasform;
    [SerializeField] private Transform rivalCardTrasform;
    
    [SerializeField] private Transform[] stack;
    [SerializeField] private Transform[] stackUnits;

    [Header("Server components")]
    [SerializeField] private PlayerNet playerNet;
    private bool animate = false;
    public bool CardSelected = false;

    private void Start()
    {
        canvas.worldCamera = Camera.main;

        // При старте скрывать все карты на столе
        foreach (Transform stk in stack)
        {
            stk.gameObject.SetActive(false);
        }
        foreach (CardParameters card in playerCards)
        {
            card.Selected = true;
            card.Card.Used = true;
        }

        for (int i = 0; i < playerCards.Length; i++)
        {
            CardHandler cardHand = playerCards[i].GetComponent<CardHandler>();
            cardHand.CardDisplacement = playerCardPositions[i].transform.position;
            cardHand.ReturnCard();

            cardHand = rivalCards[i].GetComponent<CardHandler>();
            cardHand.CardDisplacement = rivalCardPositions[i].transform.position;
            cardHand.ReturnCard();
        }
    }

    public void LocalUpdate()
    {
        for(int i = 0; i < 5; i++)
        {
            playerCards[i].SetCardEffects(playerNet.HandCards[i], i);
        }
    }

    // Метод нажатия на карту (Кнопка передает конкретную карту)
    public void OnCardSelect(RectTransform card)
    {
        StartCoroutine(CardSelectIE(card)); 
    }

    private IEnumerator CardSelectIE(RectTransform card)
    {
        card.TryGetComponent<CardParameters>(out CardParameters cardParameters);    

        if (cardParameters && !animate)
        {            
            int cardID = cardParameters.GetCardId();

            if (playerNet.Morale - playerNet.HandCards[cardID].CardCost >= 0 && playerNet.MyTurn)
            {
                cardParameters.Selected = true;
                cardParameters.Card.Used = true;
                // Перемещение карт
                StartCoroutine(CardTransition(card, card.transform.position, tablePosition.transform.position));

                // Инициализация новых позиций
                int selectedCount = 0;
                for (int i = 0; i < playerCards.Length; i++)
                {
                    if (playerCards[i].Selected)
                    {
                        for (int j = i; j < playerCards.Length; j++)
                        {
                            if (j + 1 == playerCards.Length) continue;

                            playerCards[j + 1].GetComponent<CardHandler>().CardDisplacement = playerCardPositions[j - selectedCount].position;
                        }
                        selectedCount++;
                    }
                }

                for (int i = 0; i < playerCards.Length; i++)
                {
                    if (!playerCards[i].Selected)
                    {
                        playerCards[i].GetComponent<CardHandler>().ReturnCard();
                    }
                }

                animate = true;

                while (animate)
                {
                    yield return null;
                }
                
                card.position = playerCardTrasform.position;

                playerNet.Morale -= playerNet.HandCards[cardParameters.GetCardId()].CardCost;
                playerNet.PlayerStatus(playerNet.CardSelected = true, cardID);
                playerNet.UpdateUI();
            }
            else
            {
                card.GetComponent<CardHandler>().ReturnCard();
            }
        }
        else
        {
            cardParameters.GetComponent<CardHandler>().ReturnCard();
        }

        yield return null;
    }

    public void RivalCardSelect(int index, CardData card)
    {
        rivalCards[index].Selected = true;
        StartCoroutine(RivalSelectIE(index, card));
    }

    public IEnumerator RivalSelectIE(int index, CardData card)
    {
        rivalCards[index].SetCardEffects(card, index);
        StartCoroutine(CardTransition(rivalCards[index].transform, rivalCards[index].transform.position, tablePosition.position));
        animate = true;

        while (animate)
        {
            yield return null;
        }

        rivalCards[index].transform.position = rivalCardTrasform.position;

        int counter = rivalCards.Length;
        foreach(CardParameters cardParams in rivalCards)
        {
            if (cardParams.Selected)
            {
                counter--;
            }
        }

        if (counter <= 2)
        {
            foreach(CardParameters cardParams in rivalCards)
            {
                if(cardParams.Selected)
                {
                    cardParams.gameObject.SetActive(true);
                    cardParams.GetComponent<CardHandler>().ReturnCard();
                    cardParams.Selected = false;
                }
            }
        }

        yield return null;
    }

    public void ReturnCards()
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            Debug.Log("Card returned");
            playerCards[i].Card.Used = false;
            playerCards[i].Selected = false;
            playerCards[i].gameObject.SetActive(true);

            playerCards[i].GetComponent<CardHandler>().CardDisplacement = playerCardPositions[i].transform.position;

            playerCards[i].SetCardEffects(playerNet.HandCards[i], i);
            playerCards[i].GetComponent<CardHandler>().ReturnCard();
        }
    }

    // Перемещение карты
    private IEnumerator CardTransition(Transform currentTransform, Vector3 startPos, Vector3 endPos, bool reverse = false)
    {
        Vector3 BezierPos = new Vector3(startPos.x, endPos.y, endPos.z);
        Vector3 posVector = endPos - startPos;

        float progress = 0;
        
        bool transition = true;

        while (transition) // 
        {
            Vector3 p1 = Vector3.Lerp(startPos, BezierPos, progress);
            Vector3 p2 = Vector3.Lerp(BezierPos, endPos, progress);
            Vector3 position = Vector3.Lerp(p1, p2, progress);

            currentTransform.position = position;
            progress += Time.deltaTime + 0.02f;

            if (progress >= 1)
            {
                currentTransform.position = endPos;
                transition = false;
            }

            yield return new WaitForUpdate();
        }

        // Возвращать карту к фишке и когда у игрока будет 2 карты, вернуть их обратно
        currentTransform.gameObject.SetActive(false);

        UpdateStack(currentTransform.GetComponent<CardParameters>().Card);

        animate = false;

        yield return null;
    }

    private void UpdateStack(CardData card)
    {
        for (int i = 0; i < stack.Length; i++)
        {
            if (!stack[i].gameObject.activeInHierarchy)
            {
                // Передать в этот элемент стопки характеристики выбранной карты
                stack[i].gameObject.SetActive(true);
                SetStackUnit(stackUnits[i], card);

                return;
            }
        }

        // Если все стопки активны, то они смещаются (+ смещение массива стопки)
        Transform[] varStackUnits = new Transform[stackUnits.Length];

        for (int i = stack.Length-1; i >= 0 ; i--)
        {
            if (i > 0)
            {
                stackUnits[i].SetParent(stack[i-1]);
                varStackUnits[i-1] = stackUnits[i];
            }
            else // Если последний элемент, то смещать в начало цикла
            {
                stackUnits[i].SetParent(stack[stack.Length-1]);
                varStackUnits[stack.Length-1] = stackUnits[i];
            }
        }

        // Передать в этот элемент стопки характеристики выбранной карты
        stackUnits = varStackUnits;
        SetStackUnit(stackUnits[stack.Length - 1], card);
    }

    public void SetStackUnit(Transform stackUnit, CardData playerCard)
    {
        stackUnit.TryGetComponent<CardParameters>(out CardParameters cardParam);

        if (cardParam)
        {
            cardParam.SetCardEffects(playerCard, 0); // 0 - задается индекс карты (для стопки необязательный параметр)
        }
    }

    public void SelectRandomCard()
    {
        // Выбор рандомной карты (если таймер дойдет до 0)
    }
}