using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chip", menuName = "Data")]
public class Chip : ScriptableObject
{
    public Texture ChipImage;
    
    [Header("Stats")]
    public string Name;
    public string Description;

    [Header("Cards")]
    public Card[] Cards;
}
