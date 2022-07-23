using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GlobalEventManager : MonoBehaviour
{
    public static UnityEvent<CardData[]> OnSelectCards = new UnityEvent<CardData[]>();
    public static UnityEvent<CardData> OnSelectSingleCard = new UnityEvent<CardData>();
    public static UnityEvent<Texture[]> OnLoadNftTexture = new UnityEvent<Texture[]>();
    public static UnityEvent<ChipParameters> OnSelectChip = new UnityEvent<ChipParameters>();

    public static void SendCards(CardData[] cards) // Card[] cards
    {
        OnSelectCards.Invoke(cards);
    }

    public static void SendSingleCard(CardData card)
    {
        OnSelectSingleCard.Invoke(card);
    }

    public static void SendNftTexture(Texture[] nftTexture)
    { 
        OnLoadNftTexture.Invoke(nftTexture);
    }

    public static void SendChipData(ChipParameters chipData)
    {
        OnSelectChip.Invoke(chipData);
    }
}
