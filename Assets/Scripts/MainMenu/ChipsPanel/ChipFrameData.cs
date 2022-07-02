using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class ChipFrameData : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private RawImage chipTexture;
    [SerializeField] private Text chipName;

    private ChipParameters chipData;
    private bool selectable;

    private string chipCapital;
    private string chipMorale;
    private string chipRating;

    private Text chipCapitalTxt;
    private Text chipMoraleTxt;
    private Text chipRatingTxt;

    public void Init(ChipParameters chipParam, Text capitalTxt, Text moraleTxt, Text ratingTxt, bool selectable = false)
    {
        this.selectable = selectable;
        this.chipData = chipParam;
        this.chipName.text = this.chipData.ChipName;

        chipCapital = chipParam.Capital;
        chipMorale = chipParam.Morale;
        chipRating = chipParam.Rating;

        chipCapitalTxt = capitalTxt;
        chipMoraleTxt = moraleTxt;
        chipRatingTxt = ratingTxt;

        chipTexture.texture = chipParam.ChipTexture;
    }
    public void SetChipForGame()
    {
        if (selectable)
        {
            PlayerPrefs.SetInt("chipId", chipData.Id);
            SceneManager.LoadScene(1);
        }
    }

    public void OnPointerDown(PointerEventData eventData) // При нажатии на фишку выводится список карт конкретной фишки 
    {
        if (!selectable)
        { 
            GlobalEventManager.SendCards(chipData.CardDeck);

            chipCapitalTxt.text = chipCapital;
            chipMoraleTxt.text = chipMorale;
            chipRatingTxt.text = chipRating;
        }
    }
}
