using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubtypeObject : MonoBehaviour
{
    [SerializeField] private Image imageType;
    [SerializeField] private Text valueText;

    public void UpdateUI(Sprite typeSprite, string valueType)
    {
        this.imageType.sprite = typeSprite;
        this.valueText.text = valueType;
    }
}
