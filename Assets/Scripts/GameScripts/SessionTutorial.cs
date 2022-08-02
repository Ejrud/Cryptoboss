using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SessionTutorial : MonoBehaviour
{
    [SerializeField] private GameObject[] _slides;
    [SerializeField] private int _mainMenuIndex;

    private int currentSlide = 0;

    private void Start()
    {
        ChangeSlide(currentSlide);
    }

    private void Update()
    {
        // if (Input.GetMouseButtonUp(0))
        // {
        //     currentSlide++;
        //     if (currentSlide <= _slides.Length-1)
        //     {
        //         ChangeSlide(currentSlide);
        //     }
        //     else
        //     {
        //         // Tutorial complete
        //         SceneManager.LoadScene(_mainMenuIndex);
        //     }
        // }
    }
    public void StopTutorial()
    {
        SceneManager.LoadScene(_mainMenuIndex);
    }
        
    private void ChangeSlide(int slideIndex)
    {
        foreach (GameObject slide in _slides)
        {
            slide.SetActive(false);
        }

        _slides[slideIndex].SetActive(true);
    }
}
