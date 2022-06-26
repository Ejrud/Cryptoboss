using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChipFrameData : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private RawImage chipTexture;
    [SerializeField] private Text chipName;

    private ChipParameters chipData;
    private bool selectable;

    public void Init(ChipParameters chipParam, bool selectable = false)
    {
        this.selectable = selectable;
        this.chipData = chipParam;
        this.chipName.text = this.chipData.ChipName;
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
        }
    }
}
