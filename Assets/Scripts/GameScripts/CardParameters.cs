using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardParameters : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool Selected = false; // ��������� ��� �������� �������� (���� ����� �������, �� �� ������ �������� �������)
    public bool Session = true; // ���� ����� ������������ �� � ������� ������, �� ������������ �� ��������������  ������� ����
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

    public void SetCardEffects(CardData card, int cardId)   // ��� ������ ������ � ������ ����� �������� �������������� + ����������� ��� ������������(�������� �����!!)
    {
        this.cardId = cardId;
        Card = card;
        
        GetComponent<CardAnimationController>().UpdateStats(card); // �������� �� ������������

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
        // ���� ������� ����, �� ���������� �������������� ���� � ��������� ����
        if (!Session && startPos == eventData.position)
        {
            GlobalEventManager.SendSingleCard(Card);
        }
    }

    #endregion
}
