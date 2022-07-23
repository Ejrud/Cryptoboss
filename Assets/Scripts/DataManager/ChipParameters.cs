using UnityEngine;

[System.Serializable]
public class ChipParameters
{
    public int Id;
    public string ChipName;
    public string Description;
    public string Species;
    public string Role;
    // ����������� �����
    public Texture ChipTexture;
    // ����� �����
    public CardData[] CardDeck;

    public string Morale;
    public string Rating;
    public string Capital;
}
