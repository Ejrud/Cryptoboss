using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardParameters : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool Selected = false; // Необходим для игрового процесса (Если карта выбрана, то ее нельзя повторно выбрать)
    public bool Session = true; // Если карта используется не в игровой сессии, то показываются ее характеристики  главном меню
    public CardData Card = new CardData();
    private int cardId;
    [Header("Links")]
    // [SerializeField] private CardImage cardImage;
    [SerializeField] private Image cardImage;

    [Header("Values")]
    [SerializeField] private Text cardName;
    [SerializeField] private TMP_Text capitalDamage;
    [SerializeField] private TMP_Text capitalHealth;
    [SerializeField] private TMP_Text energyCost;
    [SerializeField] private TMP_Text energyHealth;

    [Header("Sprites")]
    [SerializeField] private Sprite[] typeSprites; // 0 - damage, 1 - heal, 3 - energy, 4 - turn, 5-lique, 6-corr, 7-piv, 8-piv, 9-scam, 10-hedg fund 11- audit, 12, to moon, 13-pump

    private Vector2 startPos;

    public void SetCardEffects(CardData card, int cardId)   // При каждом раунде в каждой карте меняются характеристики + изображение для визуализации(добавить позже!!)
    {
        this.cardId = cardId;
        Card = card;
        
        GetComponent<CardAnimationController>().UpdateStats(card); // Возможно не используется

        UpdateUI();
    }

    public int GetCardId()
    {
        return cardId;
    }

    private void UpdateUI()
    {
        switch (Card.Type)
        {
            case "damage":
                cardImage.sprite = typeSprites[0];
            break;
            case "heal":
                cardImage.sprite = typeSprites[1];
            break;
            case "energy":
                cardImage.sprite = typeSprites[3];
            break;
            case "turn around":
                cardImage.sprite = typeSprites[4];
            break;
            case "liquidation":
                cardImage.sprite = typeSprites[5];
            break;
            case "correction":
                cardImage.sprite = typeSprites[6];
            break;
            case "pivot":
                cardImage.sprite = typeSprites[7];
            break;
            case "scam":
                cardImage.sprite = typeSprites[9];
            break;
            case "hedge fund":
                cardImage.sprite = typeSprites[10];
            break;
            case "audit":
                cardImage.sprite = typeSprites[11];
            break;
            case "to the moon":
                cardImage.sprite = typeSprites[12];
            break;
            case "pump":
                cardImage.sprite = typeSprites[13];
            break;
        }

        if (Card.Type == "damage" || Card.Type == "heal" || Card.Type == "energy")
        {
            capitalDamage.text = "-" + Card.CapitalDamage.ToString();
            capitalHealth.text = "+" + Card.CapitalEarnings.ToString();
            energyCost.text = "-" + Card.CardCost.ToString();
            energyHealth.text = "+" + Card.EnergyHealth.ToString();
        }
        else
        {
            capitalDamage.text = "";
            capitalHealth.text = "";
            energyCost.text = "";
            energyHealth.text = "";
        }

        cardName.text = Card.Name;
        // cardImage.OnSetCard(Card.Name);
    }

    #region CardParameters(MainMenu)
    public void OnPointerDown(PointerEventData eventData) 
    {
        startPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Если главное меню, то показывать характеристики карт в отдельном окне
        if (!Session && startPos == eventData.position)
        {
            GlobalEventManager.SendSingleCard(Card);
        }
    }

    #endregion
}
