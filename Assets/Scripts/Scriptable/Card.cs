using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardParameters", menuName = "Data")]
public class Card : ScriptableObject
{
    public Sprite CardImage;

    [Header("Parameteres")]
    public float Attack;
    public float Defence;
    public string Description;
}
