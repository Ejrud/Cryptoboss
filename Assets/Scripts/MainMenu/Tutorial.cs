using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tutorial : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Slide[] slides;

    [Header("UI")]
    [SerializeField] private Transform _frontLayerTransform; // Необходим для выделения объекта среди других элементов интерфейса
    [SerializeField] private RectTransform _teddyTransform; // 
    [SerializeField] private Text _description;
    [SerializeField] private GameObject _clicklTextObj;
    private Transform _oldTransform; // Изначальный transform выделяемого элемента
    private GameObject NextSlide;
    private GameObject PreviousSlide;
    private string _userName;
    private bool _tutorial;
    private bool _prepareNextSlide;
    private bool _returnToMain;
    private bool _inactiveCurrentSlide;
    private int _slideIndex = 0;
    // return object
    private Transform _previousTransform;
    private Transform _selectedElement;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void StartTutorial(string userName)
    {
        _userName = userName;
        _tutorial = true;
        SetNextSlide();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_tutorial && !_inactiveCurrentSlide)
        {
            if (_slideIndex < slides.Length - 1)
            {
                _slideIndex++;
                SetNextSlide();
            }
            else
            {
                _selectedElement.SetParent(_previousTransform);
                gameObject.SetActive(false);
            }
        }
    }

    public void Next()
    {
        _slideIndex++;
        _inactiveCurrentSlide = false;
        SetNextSlide();
    }

    private void SetNextSlide()
    {
        if (slides[_slideIndex].Hide)
            _teddyTransform.gameObject.SetActive(false);
        else
            _teddyTransform.gameObject.SetActive(true);

        if (_previousTransform != null && _selectedElement != null) // Возврат в изначальный transform
        {
            _selectedElement.SetParent(_previousTransform);
        }

        if (_prepareNextSlide)
        {
            NextSlide.SetActive(true);
            _prepareNextSlide = false;
        }
        else if (_returnToMain)
        {
            PreviousSlide.SetActive(false);
            _returnToMain = false;
        }

        if (slides[_slideIndex].moveToNextWindow)
        {
            _prepareNextSlide = true;
            NextSlide = slides[_slideIndex].DescribedWindow;
        }
        else if (slides[_slideIndex].moveToPreviousWindow)
        {
            _returnToMain = true;
            PreviousSlide = slides[_slideIndex].PreviousWindow;
        }
        
        if (slides[_slideIndex]._reverseTeddy)
        {
            _teddyTransform.anchorMax = new Vector2(0, 0);
            _teddyTransform.anchorMin = new Vector2(0, 0);
            _teddyTransform.localScale = new Vector3(-1, 1, 1);
            _teddyTransform.anchoredPosition = new Vector3(0, 0, 0);
            _description.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            _teddyTransform.anchorMax = new Vector2(1, 0);
            _teddyTransform.anchorMin = new Vector2(1, 0);
            _teddyTransform.localScale = new Vector3(1, 1, 1);
            _teddyTransform.anchoredPosition = new Vector3(0, 0, 0);
            _description.transform.localScale = new Vector3(1, 1, 1);
        }

        if (slides[_slideIndex].Inactive)
        {
            _inactiveCurrentSlide = true;
            _clicklTextObj.SetActive(false);
        }
        else 
        {
            _clicklTextObj.SetActive(true);
        }

        // Сохранение прошлой позиции
        _previousTransform = slides[_slideIndex].PreviousTransform;
        // Выделяемый объект
        _selectedElement = slides[_slideIndex].DescribedTransform;
        // Выделение элемента интерфейса
        if (slides[_slideIndex].DescribedTransform != null)
            slides[_slideIndex].DescribedTransform.SetParent(_frontLayerTransform);

        _description.text = slides[_slideIndex].text[0];

        if (_slideIndex == 0)
        {
            _description.text = slides[_slideIndex].text[0] + _userName + slides[_slideIndex].text[1];
        }

    }
}

[System.Serializable]
public class Slide
{
    public string slideName;

    [Header("Slide settings")]
    public bool Hide;
    public bool _reverseTeddy;
    public bool Inactive;
    public Transform PreviousTransform;
    public Transform DescribedTransform;
    public string[] text;
    
    [Header("Next window")]
    public bool moveToNextWindow;
    public GameObject DescribedWindow;

    [Header("Previous window")]
    public bool moveToPreviousWindow;
    public GameObject PreviousWindow;
}
