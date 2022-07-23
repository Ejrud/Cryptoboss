using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ChipDescription : MonoBehaviour, IPointerDownHandler
{
    [Header("UI")]
    [SerializeField] private GameObject _chipDescriptionObj;
    [SerializeField] private RawImage _chipImage;
    [SerializeField] private TMP_Text _species;
    [SerializeField] private TMP_Text _role;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private Text _name;
    private ChipParameters _chipData;

    private void Start()
    {
        GlobalEventManager.OnSelectChip.AddListener(UpdateChipDescription);
    }

    public void OpenDescription()
    {
        if (_chipData != null)
        {
            _chipImage.texture = _chipData.ChipTexture;
            _species.text = _chipData.Species;
            _role.text = _chipData.Role;
            _description.text = _chipData.Description;
            _name.text = _chipData.ChipName;

            _chipDescriptionObj.SetActive(true);
        }
    }
    public void CloseDescription()
    {
        _chipData = null;
        _chipDescriptionObj.SetActive(false);
    }

    private void UpdateChipDescription(ChipParameters chipData)
    {
        _chipData = chipData;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CloseDescription();
    }
}
