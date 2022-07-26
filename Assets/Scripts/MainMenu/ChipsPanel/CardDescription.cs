using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardDescription : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Tutorial tutorial;

    [Header("Descriptions")]
    [SerializeField] private string[] _descriptions; // 0 - turn around/ 1 - liq/ 2 - correc/ 3 - piv 100 2/  4 - piv 2 100/ 5 - scam/ 6 - hedge/ 7 - audit/ 8 - to the moon/ 9 - pump

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
        int typeIndex = data.CapitalDamage;

        cardParams.SetCardEffects(data, 0);

        UpdateUI(value, typeIndex, data.Name, "Not found");
    }

    private void UpdateUI(int typeValue, int typeIndex, string cardName, string description)
    {
        this.description.text = description;
        string name = cardName;
        cardName = name.Substring(0, 1).ToUpper() + name.Remove(0, 1).ToLower();

        switch (cardName)
        {
            case "Turn around":
                this.description.text = _descriptions[0];
                break;

            case "Liquidation":
                this.description.text = _descriptions[1];
                break;

            case "Correction":
                this.description.text = _descriptions[2];
                break;

            case "Pivot":
                if(typeIndex > 0)
                {
                    this.description.text = _descriptions[3];
                }
                else
                {
                    this.description.text = _descriptions[4];
                }
                break;

            case "Scam":
                this.description.text = _descriptions[5];
                break;

            case "Hedge fund":
                this.description.text = _descriptions[6];
                break;

            case "Audit":
                this.description.text = _descriptions[7];
                break;

            case "To the moon":
                this.description.text = _descriptions[8];
                break;

            case "Pump":
                this.description.text = _descriptions[9];
                break;

            default:
                this.description.text = "Not found";
                break;
        }

        this.typeValue.text = typeValue.ToString();
    }

    // ��������� ���� ���� � ������� ���� ���� ��������� ������� �� �����
    public void OnPointerDown(PointerEventData eventData)
    {
        cardDescriptionObj.SetActive(false);
    }

    public void CloseDescription()
    {
        if (tutorial._tutorial)
        {
            tutorial.Next();
        }

        cardDescriptionObj.SetActive(false);
    }
}
