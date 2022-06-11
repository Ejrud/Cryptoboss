using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardImage : MonoBehaviour
{
    [Header("Card image")]
    [SerializeField] private Image cardImage;
    
    [Header("Image massive")]
    [SerializeField] private Sprite[] imageTypes;

    public void OnSetCard(string typeName)
    {
        for (int i = 0; i< imageTypes.Length; i++)
        {
            if (imageTypes[i].name == typeName)
            {
                cardImage.sprite = imageTypes[i];
            }
        }
    }
}
