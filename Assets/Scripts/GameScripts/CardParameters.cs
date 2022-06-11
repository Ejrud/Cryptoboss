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
    [SerializeField] private CardImage cardImage;

    [Header("Values")]
    [SerializeField] private Text cardName;
    [SerializeField] private TMP_Text capitalDamage;
    [SerializeField] private TMP_Text capitalHealth;
    [SerializeField] private TMP_Text energyCost;
    [SerializeField] private TMP_Text energyHealth;

    [Header("Sprites")]
    [SerializeField] private Sprite[] typeSprites; // 0 - damage, 1 - heal, 3 - energy, 4 - defense
    [SerializeField] private Image borderColor;
    [SerializeField] private Sprite[] colors; // 0 - damage, 1 - heal, 3 - energy, 4 - defense

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
        capitalDamage.text = "-" + Card.CapitalDamage.ToString();
        capitalHealth.text = "+" + Card.CapitalEarnings.ToString();
        energyCost.text = "-" + Card.CardCost.ToString();
        energyHealth.text = "+" + Card.EnergyHealth.ToString();

        cardName.text = Card.Name;
        cardImage.OnSetCard(Card.Name);
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
