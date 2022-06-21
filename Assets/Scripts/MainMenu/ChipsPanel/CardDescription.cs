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

        // ���� �������� ����� ��� ����� �� ��������� ���� �����
        int value = 0;
        int typeIndex = 0;

        cardParams.SetCardEffects(data, 0);

        UpdateUI(value, typeIndex, data.Name, "Not found");
    }

    private void UpdateUI(int typeValue, int typeIndex, string cardName, string description)
    {
        this.description.text = description;
        this.typeValue.text = typeValue.ToString();
    }

    // ��������� ���� ���� � ������� ���� ���� ��������� ������� �� �����
    public void OnPointerDown(PointerEventData eventData)
    {
        cardDescriptionObj.SetActive(false);
    }
}
