using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ChipData")]

public class ChipData : ScriptableObject
{
    public int Id;
    public string ChipName;
    // Изображение фишки
    public Texture ChipTexture;
    // Карты фишки
    public CardData[] CardDeck;
}

