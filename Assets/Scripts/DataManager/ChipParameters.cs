using UnityEngine;

[System.Serializable]
public class ChipParameters
{
    public int Id;
    public string ChipName;
    // ����������� �����
    public Texture ChipTexture;
    // ����� �����
    public CardData[] CardDeck;

    public string Morale;
    public string Rating;
    public string Capital;
}
