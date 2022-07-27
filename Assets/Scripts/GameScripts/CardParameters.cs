using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardParameters : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Tutorial tutorial;
    public bool Selected = false; // Необходим для игрового процесса (Если карта выбрана, то ее нельзя повторно выбрать)
    public bool Session = true; // Если карта используется не в игровой сессии, то показываются ее характеристики  главном меню
    public bool Audited = false; // 
    public CardData Card = new CardData();
    private int cardId;
    [Header("Links")]
    // [SerializeField] private CardImage cardImage;
    [SerializeField] private Image cardImage;
    [SerializeField] private GameObject bossyObj;
    [SerializeField] private GameObject energyObj;
    [SerializeField] private GameObject cardBackplate;

    [Header("Values")]
    [SerializeField] private Text cardName;
    [SerializeField] private TMP_Text capitalDamage;
    [SerializeField] private TMP_Text capitalHealth;
    [SerializeField] private TMP_Text energyCost;
    [SerializeField] private TMP_Text energyHealth;

    [Header("Sprites")]
    [SerializeField] private Sprite[] typeSprites; // 0 - damage, 1 - heal, 3 - energy, 4 - turn, 5-lique, 6-corr, 7-piv (eng), 8-piv (capit), 9-scam, 10-hedg fund 11- audit, 12, to moon, 13-pump

    [SerializeField] private Sprite unknownSprite;

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

    // rivalCard 
    public void OpenCard()
    {
        cardBackplate.SetActive(false);
    }
    public void CloseCard()
    {
        cardImage.sprite = unknownSprite;
        cardBackplate.SetActive(true);
    }

    private void UpdateUI()
    {
        bossyObj.gameObject.SetActive(true);
        energyObj.gameObject.SetActive(true);
        
        switch (Card.Type)
        {
            case "damage":
                cardImage.sprite = typeSprites[0];
            break;
            case "heal":
                cardImage.sprite = typeSprites[1];
            break;
            case "energy":
                cardImage.sprite = typeSprites[2];
            break;
            case "joker":
                string name = Card.Name;
                Card.Name = name.Substring(0, 1).ToUpper() + name.Remove(0, 1).ToLower();

                for (int i = 0; i < typeSprites.Length; i++)
                {
                    if (typeSprites[i].name == Card.Name)
                    {
                        cardImage.sprite = typeSprites[i];
                        
                        if (Card.Name == "Pivot" && Card.CapitalDamage != 0)
                        {
                            cardImage.sprite = typeSprites[6]; // energy health
                        }
                        else if (Card.Name == "Pivot" && Card.CapitalDamage == 0)
                        {
                            cardImage.sprite = typeSprites[7]; // capital health
                        }
                        break;
                    }
                }

                bossyObj.gameObject.SetActive(false);
                energyObj.gameObject.SetActive(false);
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
        if (!Session)
        {
            if (tutorial._tutorial)
            {
                tutorial.Next();
            }
            // Если главное меню, то показывать характеристики карт в отдельном окне
            if (startPos == eventData.position)
            {
                GlobalEventManager.SendSingleCard(Card);
            }
        }
    }

    #endregion
}
