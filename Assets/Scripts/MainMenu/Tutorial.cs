using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class Tutorial : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private User user;
    [SerializeField] private UserData userData;
    [SerializeField] private Slide[] slides;
    [SerializeField] private int nameLength = 18;

    [Header("UI")]
    [SerializeField] private InputField userName;
    [SerializeField] private Transform _frontLayerTransform; // Необходим для выделения объекта среди других элементов интерфейса
    [SerializeField] private RectTransform _teddyTransform; // 
    [SerializeField] private Text _description;
    [SerializeField] private GameObject _clicklTextObj;
    [SerializeField] private GameObject _NickNameWindow;

    [Header("DescriptionPositions")]
    [SerializeField] private Transform _descriptionTransform;
    [SerializeField] private Transform[] _positions; // 0 - default position 1 - offset position
    private Transform _oldTransform; // Изначальный transform выделяемого элемента
    private GameObject NextSlide;
    private GameObject PreviousSlide;
    private string _userName;
    public bool _tutorial;
    private bool _prepareNextSlide;
    private bool _returnToMain;
    private bool _inactiveCurrentSlide;
    private int _slideIndex = 0;
    // return object
    private Transform _previousTransform;
    private Transform _selectedElement;
    private string editUrl = "https://cryptoboss.win/game/back/editProfile.php"; // a0664627.xsph.ru/cryptoboss_back/editProfile.php   // https://cryptoboss.win/game/back/editProfile.php

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void PrepareTutorial()
    {
        _NickNameWindow.SetActive(true);
    }

    public void StartTutorial(string userName)
    {
        _NickNameWindow.SetActive(false);
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
                _tutorial = false;
                _selectedElement.SetParent(_previousTransform);
                gameObject.SetActive(false);
            }
        }
    }

    public void Next()
    {
        if (_tutorial)
        {
            _slideIndex++;
            _inactiveCurrentSlide = false;
            SetNextSlide();
        }
    }

    private void SetNextSlide()
    {
        // Text offset
        if (slides[_slideIndex].Offset)
            _descriptionTransform.position = _positions[1].position;
        else
            _descriptionTransform.position = _positions[0].position;
        
        // Hide bear
        if (slides[_slideIndex].Hide)
            _teddyTransform.gameObject.SetActive(false);
        else
            _teddyTransform.gameObject.SetActive(true);

        // Return to default transform
        if (_previousTransform != null && _selectedElement != null) 
            _selectedElement.SetParent(_previousTransform);
        
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

    public void SetName()
    {
        StartCoroutine(SendForm());
    }

    private IEnumerator SendForm()
    {
        WWWForm form = new WWWForm();

        string name = userName.text;

        if (name.Length > nameLength)
        {
            name = name.Remove(nameLength);
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            yield return null;
        }
        
        form.AddField("UserName", name);
        form.AddField("UserWallet", user.Wallet);
        form.AddField("Id", Convert.ToInt32(user.UserID));

        using (UnityWebRequest www = UnityWebRequest.Post(editUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                user.UserName = name;
                userData.UpdateUI();
                StartTutorial(user.UserName);
            }
            else
            {
                Debug.Log(www.error);
            }
        }

        yield return null;
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
    public bool Offset; 
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
