using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ChipData")]

public class ChipData : ScriptableObject
{
    public int Id;
    public string ChipName;
    // ����������� �����
    public Texture ChipTexture;
    // ����� �����
    public CardData[] CardDeck;
}

