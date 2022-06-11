using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGenerator : MonoBehaviour
{
    [SerializeField] private bool generateCard = false;
    [SerializeField] private ChipData[] chipData;

    private void Start()
    {
        // if (generateCard)
        // {
        //     for (int i = 0; i < chipData.Length; i++)
        //     {
        //         chipData[i].CardDeck = new CardData[40];
        //         chipData[i].CardDeck = GetDebugCards(40);
        //     }
        // }
    }

    // private CardData[] GetDebugCards(int length)
    // {
    //     // CardData[] cardDeck = new CardData[length];

    //     // for (int i = 0; i < cardDeck.Length; i++)
    //     // {
    //     //     CardData card = new CardData();

    //     //     card.Guid = Random.Range(1000, 9999).ToString();
    //     //     card.Name = "RandomCardName(" + Random.Range(0, 1000).ToString() + ")";
    //     //     card.EnergyDamage = Random.Range(10, 50);
    //     //     card.CapitalDamage = Random.Range(10, 50);
    //     //     card.EnergyHealth = Random.Range(1, 10);
    //     //     card.CapitalEarnings = Random.Range(1, 10);
    //     //     card.DamageResistance = Random.Range(1, 10);
    //     //     card.CardCost = Random.Range(10, 50);
    //     //     card.Version = "1";

    //     //     cardDeck[i] = card;
    //     // }

    //     // return cardDeck;
    // }

    //for (int i = 0; i < chipData.Length; i++)
    //{
    //    chipData[i].Guid = new string[40];
    //    chipData[i].Name = new string[40];
    //    chipData[i].EnergyDamage = new int[40];
    //    chipData[i].CapitalDamage = new int[40];
    //    chipData[i].EnergyHealth = new int[40];
    //    chipData[i].CapitalEarnings = new int[40];
    //    chipData[i].DamageResistance = new int[40];
    //    chipData[i].CardCost = new int[40];
    //    chipData[i].Version = new string[40];

    //    for (int j = 0; j < 40; j++)
    //    {
    //        chipData[i].Guid[j] = Random.Range(1000, 9999).ToString();
    //        chipData[i].Name[j] = "CardName(" + Random.Range(0, 1000).ToString() + ")";
    //        chipData[i].EnergyDamage[j] = Random.Range(10, 50);
    //        chipData[i].CapitalDamage[j] = Random.Range(10, 50);
    //        chipData[i].EnergyHealth[j] = Random.Range(1, 10);
    //        chipData[i].CapitalEarnings[j] = Random.Range(1, 10);
    //        chipData[i].DamageResistance[j] = Random.Range(1, 10);
    //        chipData[i].CardCost[j] = Random.Range(10, 50);
    //        chipData[i].Version[j] = "1";
    //    }
    //}

    //else
    //{
    //    for (int i = 0; i < 40; i++)
    //    {
    //        Debug.Log(chipData.Name[i]);
    //    }
    //}
}
