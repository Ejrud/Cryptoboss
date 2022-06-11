using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardFrameData : MonoBehaviour
{
    [SerializeField] private Image cardImage;

    private CardData currentCard;
    private Sprite cardSprite;

    private string description;
    private float attack;
    private float defence;
    private float address;

    public void Init(CardData currentCard)
    {
        this.currentCard = currentCard;

        //cardSprite = _currentCard.CardImage;
        //cardImage.sprite = cardSprite;
        description = currentCard.Name;
        attack = currentCard.CapitalDamage;
        defence = currentCard.DamageResistance;
    }

}
