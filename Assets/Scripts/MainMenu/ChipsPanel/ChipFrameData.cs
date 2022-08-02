using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class ChipFrameData : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RawImage chipTexture;
    [SerializeField] private Text chipName;
    [SerializeField] private Image chipBgImage;
    [SerializeField] private Sprite chipSelected;
    [SerializeField] private Sprite chipDefault;

    private ChipParameters chipData;
    private ShowingChipsController chipController;
    private bool selectable;

    private string chipCapital;
    private string chipMorale;
    private string chipRating;

    private Text chipCapitalTxt;
    private Text chipMoraleTxt;
    private Text chipRatingTxt;
    private Vector2 startPos;
    private int chipIndex;

    public void Init(ShowingChipsController chipController, ChipParameters chipParam, Text capitalTxt, Text moraleTxt, Text ratingTxt, int chipIndex, bool selectable = false)
    {
        this.selectable = selectable;
        this.chipData = chipParam;
        this.chipName.text = this.chipData.ChipName;
        this.chipController = chipController;
        this.chipIndex = chipIndex;

        chipCapital = chipParam.Capital;
        chipMorale = chipParam.Morale;
        chipRating = chipParam.Rating;
        if (string.IsNullOrWhiteSpace(chipRating)) chipRating = "0";
        if (string.IsNullOrWhiteSpace(chipMorale)) chipMorale = "0";
        if (string.IsNullOrWhiteSpace(chipCapital)) chipCapital = "0";


        chipCapitalTxt = capitalTxt;
        chipMoraleTxt = moraleTxt;
        chipRatingTxt = ratingTxt;

        chipTexture.texture = chipParam.ChipTexture;

        chipBgImage.sprite = chipDefault;
    }

    public void SelectFirstChip()
    {
        chipController.ResetChips();
        GlobalEventManager.SendCards(chipData.CardDeck);
        GlobalEventManager.SendChipData(chipData);

        chipBgImage.color = Color.white;
        chipTexture.color = Color.white;

        chipCapitalTxt.text = chipCapital;
        chipMoraleTxt.text = chipMorale;
        chipRatingTxt.text = chipRating;
    }

    public void SetChipForGame()
    {
        if (selectable)
        {
            PlayerPrefs.SetInt("chipId", chipData.Id);
            SceneManager.LoadScene(1);
        }
    }
    
    public void OnPointerDown(PointerEventData eventData) 
    {
        startPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Если главное меню, то показывать характеристики карт в отдельном окне
        if (startPos == eventData.position && !selectable)
        {
            chipController.ResetChips();
            GlobalEventManager.SendCards(chipData.CardDeck);
            GlobalEventManager.SendChipData(chipData);

            chipBgImage.color = Color.white;
            chipTexture.color = Color.white;

            chipCapitalTxt.text = chipCapital;
            chipMoraleTxt.text = chipMorale;
            chipRatingTxt.text = chipRating;

            chipController.SelectedChipID = chipIndex;
        }
    }

    public void Reset(bool first = false)
    {
        if (first)
        {
            chipTexture.color = Color.white;
            chipBgImage.color = Color.white;
        }
        else
        {
            chipTexture.color = Color.gray;
            chipBgImage.color = Color.gray;
        }
    }
}
