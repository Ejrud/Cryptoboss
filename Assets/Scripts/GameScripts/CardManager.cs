using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Transform[] auditPositions;

    [Header("Card table")]
    [SerializeField] private Transform tablePosition;
    [SerializeField] private Transform playerCardTrasform;
    [SerializeField] private Transform rivalCardTrasform;
    
    [SerializeField] private Transform[] stack;
    [SerializeField] private Transform[] stackUnits;

    [Header("Server components")]
    [SerializeField] private PlayerNet playerNet;
    public bool Animate = false;
    public bool CardSelected = false;

    private Vector2 _screenSize;

    private void Start()
    {
        _screenSize = new Vector2(Screen.width, Screen.height);
        
        canvas.worldCamera = Camera.main;

        // При старте скрывать все карты на столе
        foreach (Transform stk in stack)
        {
            stk.gameObject.SetActive(false);
        }

        StartCoroutine(UpdateCardPositions(false, true));
    }

    private void LateUpdate()
    {
        if (new Vector2(Screen.width, Screen.height) != _screenSize)
        {
            _screenSize = new Vector2(Screen.width, Screen.height);
            
            // Обновление позиций карт
            StartCoroutine(UpdateCardPositions());
            Debug.Log("Screen was resized");
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

        if (cardParameters && !Animate)
        {            
            int cardID = cardParameters.GetCardId();

            if (playerNet.Morale - playerNet.HandCards[cardID].CardCost >= 0 && playerNet.MyTurn)
            {
                cardParameters.Selected = true;
                cardParameters.Card.Used = true;
                // Перемещение карт
                StartCoroutine(CardTransition(card, card.transform.position, tablePosition.transform.position));

                // Инициализация новых позиций
                ReposPlayerCards();

                Animate = true;

                while (Animate)
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

    public void AuditRivalCards(CardData[] rivalHand)
    {
        for (int i = 0; i < rivalCards.Length; i++)
        {
            if (rivalCards[i].Selected) continue;
            
            rivalCards[i].SetCardEffects(rivalHand[i], i);
            rivalCards[i].Audited = true;
            rivalCards[i].OpenCard();
        }

        StartCoroutine(UpdateCardPositions(true));
    }

    public void DisableAudit()
    {
        for (int i = 0; i < rivalCards.Length; i++)
        {
            rivalCards[i].Audited = false;
            if (rivalCards[i].Selected) continue;
            
            rivalCards[i].CloseCard();
        }
    }

    public void RivalCardSelect(int index, CardData card)
    {
        rivalCards[index].Selected = true;
        rivalCards[index].Audited = false;
        StartCoroutine(RivalSelectIE(index, card));
    }

    public IEnumerator RivalSelectIE(int index, CardData card)
    {
        rivalCards[index].SetCardEffects(card, index);
        StartCoroutine(CardTransition(rivalCards[index].transform, rivalCards[index].transform.position, tablePosition.position));
        Transform[] cellPosition;
        Animate = true;

        while (Animate)
        {
            yield return null;
        }

        bool audit = false;

        for (int i = 0; i < rivalCards.Length; i++)
        {
            if (rivalCards[i].Audited) audit = true; 
        } 

        if (audit)
        {
            cellPosition = auditPositions;
        }
        else
        {
            cellPosition = rivalCardPositions;
        }

        // Смещение карт в пустые ячейки
        int selectedCount = 0;

        for (int i = 0; i < rivalCards.Length; i++)
        {
            if (rivalCards[i].Selected)
            {
                for (int j = i; j < rivalCards.Length; j++)
                {
                    if (j + 1 == rivalCards.Length) continue;

                    rivalCards[j + 1].GetComponent<CardHandler>().CardDisplacement = cellPosition[j - selectedCount].position;
                    rivalCards[j + 1].GetComponent<CardHandler>().IndexPosition = j - selectedCount;
                }
                selectedCount++;
            }
        }

        for (int i = 0; i < rivalCards.Length; i++)
        {
            if (!rivalCards[i].Selected)
            {
                rivalCards[i].GetComponent<CardHandler>().ReturnCard();
            }
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
            
            for (int i = 0; i < rivalCards.Length; i++)
            {
                rivalCards[i].GetComponent<CardHandler>().CardDisplacement = cellPosition[i].position;
                
                if(rivalCards[i].Selected)
                {
                    rivalCards[i].gameObject.SetActive(true);
                    rivalCards[i].GetComponent<CardHandler>().ReturnCard();
                    rivalCards[i].Selected = false;
                    rivalCards[i].CloseCard(); // Скрыть карту если она изначально была показана аудитом
                }
                else
                {
                    rivalCards[i].GetComponent<CardHandler>().ReturnCard();
                }
            }

            selectedCount = rivalCards.Length;
            for (int i = 0; i < rivalCards.Length; i++)
            {
                if (!rivalCards[i].Audited) selectedCount--;
            }

            if (selectedCount <= 0)
            {
                StartCoroutine(UpdateCardPositions(false, false, true));
            }
        }

        yield return null;
    }

    public void ReturnCards()
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (!playerNet.HandCards[i].Used)
            {
                playerCards[i].Card.Used = false;
                playerCards[i].Selected = false;
                playerCards[i].gameObject.SetActive(true);

                playerCards[i].GetComponent<CardHandler>().CardDisplacement = playerCardPositions[i].transform.position;

                playerCards[i].SetCardEffects(playerNet.HandCards[i], i);
                playerCards[i].GetComponent<CardHandler>().ReturnCard();
                playerCards[i].GetComponent<CardHandler>().IndexPosition = i;
            }
            else
            {
                playerCards[i].gameObject.SetActive(false);
                playerCards[i].Selected = true;
            }
        }

        ReposPlayerCards();
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

        Animate = false;

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

    private IEnumerator UpdateCardPositions(bool audit = false, bool reset = false, bool dontPlayer = false)
    {
        Transform[] rivalPositions = new Transform[0];
        yield return new WaitForSeconds(.1f); // ГОвнокод

        if (reset)
        {
            for (int i = 0; i < playerCardPositions.Length; i++)
            {
                playerCards[i].GetComponent<CardHandler>().IndexPosition = i;
            }
        }

        if (audit)
        {
            rivalPositions = auditPositions;
        }
        else
        {
            rivalPositions = rivalCardPositions;
        }
            
        for (int i = 0; i < playerCards.Length; i++)
        {
            CardHandler cardHand = playerCards[i].GetComponent<CardHandler>();

            if (!dontPlayer)
            {
                cardHand.CardDisplacement = playerCardPositions[cardHand.IndexPosition].transform.position;
            
                if (!playerCards[i].Selected)
                {
                    cardHand.ReturnCard();
                }
            }

            cardHand = rivalCards[i].GetComponent<CardHandler>();
            cardHand.CardDisplacement = rivalPositions[i].transform.position;
            
            if (!rivalCards[i].Selected)
            {
                cardHand.ReturnCard();
            }
        }

        yield return null;
    }

    private void ReposPlayerCards()
    {
        int selectedCount = 0;
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (playerCards[i].Selected)
            {
                for (int j = i; j < playerCards.Length; j++)
                {
                    if (j + 1 == playerCards.Length) continue;

                    playerCards[j + 1].GetComponent<CardHandler>().CardDisplacement = playerCardPositions[j - selectedCount].position;
                    playerCards[j + 1].GetComponent<CardHandler>().IndexPosition = j - selectedCount;
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
    }
}