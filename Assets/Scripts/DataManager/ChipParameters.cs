using UnityEngine;

[System.Serializable]
public class ChipParameters
{
    public int Id;
    public string ChipName;
    // Изображение фишки
    public Texture ChipTexture;
    // Карты фишки
    public CardData[] CardDeck;
}
