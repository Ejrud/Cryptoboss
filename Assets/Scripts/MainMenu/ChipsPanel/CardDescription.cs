using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardDescription : MonoBehaviour, IPointerDownHandler
{
    [Header("UI")]
    [SerializeField] private Text typeValue;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private GameObject cardDescriptionObj;


    [Header("CardTypes")]
    [SerializeField] private GameObject[] types; // 0 - damage, 1 - heal, 3 - energy, 4 - defense
    [SerializeField] private CardParameters cardParams;
    
    private void Start()
    {
        GlobalEventManager.OnSelectSingleCard.AddListener(CardCharacteristics);

        cardDescriptionObj.SetActive(false);
    }

    private void CardCharacteristics(CardData data)
    {
        cardDescriptionObj.SetActive(true);

        // цикл проверки какой тип карты по значениям этой карты
        int value = 0;
        int typeIndex = 0;

        cardParams.SetCardEffects(data, 0);

        // В card data передавать тип карты и от нее делать границу
        // if (data.Type == "damage") 
        // {
        //     typeIndex = 0;
        //     value = data.CapitalDamage;
        //     borderImage.sprite = colors[0];
        // }
        // else if (data.Type == "heal")
        // {
        //     typeIndex = 1;
        //     value = data.CapitalEarnings;
        //     borderImage.sprite = colors[1];
        // } 
        // else if (data.Type == "energy")
        // {
        //     typeIndex = 2;
        //     value = data.EnergyHealth;
        //     borderImage.sprite = colors[2];
        // }
        // else if (data.Type == "defense")
        // {
        //     typeIndex = 3;
        //     value = data.DamageResistance;
        //     borderImage.sprite = colors[3];
        // }

        UpdateUI(value, typeIndex, data.Name, "Not found");
    }

    private void UpdateUI(int typeValue, int typeIndex, string cardName, string description)
    {
        // cardsImage.OnSetCard(cardName);
        // this.cardName.text = cardName;
        // typeImage.sprite = typeImages[typeIndex];

        // for (int i = 0; i < types.Length; i++)
        // {
        //     types[i].SetActive(false);
        // }
        // types[typeIndex].SetActive(true);
        this.description.text = description;
        this.typeValue.text = typeValue.ToString();
    }

    // Закрывать окно если в обласит окна было совершено нажатие на экран
    public void OnPointerDown(PointerEventData eventData)
    {
        cardDescriptionObj.SetActive(false);
    }
}
