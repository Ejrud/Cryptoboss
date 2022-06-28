using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private SceneLoadController sceneLoadController;
    [SerializeField] private RectTransform[] areas;

    // about chips
    [SerializeField] private GameObject selectChipWindow;
    [SerializeField] private GameObject chipFramePrefab;
    [SerializeField] private Transform chipWindowTransform;
    [SerializeField] private User user;

    private int sceneToLoad; // Индекс загрузки сцены

    private int Clicks; // 

    private Vector2 pos; // 
    private Vector2 size; // 

    private bool chipLoaded;

    private void Start()
    {
        sceneToLoad = 1;
        PlayerPrefs.SetString("GameMode", "one"); // Менять при выборе сцены
    }

    public void PanelActivate(GameObject objectToClose) // 
    {
        objectToClose.SetActive(!objectToClose.activeSelf);
    }

    public void QuitMethod() // 
    {
        Application.Quit();
    }

    public void PrepareLoadGame()
    {
        selectChipWindow.SetActive(true);

        if (!chipLoaded)
        {
            for (int i = 0; i < user.ChipParam.Count; i++)
            {
                GameObject chipFrame = Instantiate(chipFramePrefab, new Vector3(0, 0, 0), Quaternion.identity);

                chipFrame.transform.SetParent(chipWindowTransform.transform);
                chipFrame.GetComponent<ChipFrameData>().Init(user.ChipParam[i], true);
                chipFrame.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }

            chipLoaded = true;
        }
        
    }

    public void LoadGame()
    {
        sceneLoadController.LoadGame(sceneToLoad);
    }

    private void ClicksCounter() // 
    {
        switch (Clicks)
        {
            case -3:
                sceneToLoad = 1;
                Clicks = 0;
                break;

            case -2:
                sceneToLoad = 2;
                break;

            case -1:
                sceneToLoad = 3;
                break;

            case 0:
                sceneToLoad = 1;
                break;

            case 1:
                sceneToLoad = 2;
                break;

            case 2:
                sceneToLoad = 3;
                break;

            case 3:
                sceneToLoad = 1;
                Clicks = 0;
                break;
        }
    }

    private void ArenesMethod(int x, int y) // 
    {
        areas[x].anchoredPosition = areas[y].anchoredPosition;
        areas[x].sizeDelta = areas[y].sizeDelta;
        areas[x] = areas[y];
    }

    public void NextArena(bool Right) // 
    {
        // if (Right)
        // {
        //     Clicks++;
        //     pos = areas[0].anchoredPosition;
        //     size = areas[0].sizeDelta;
        //     areas[3] = areas[0];

        //     ArenesMethod(0, 1);

        //     ArenesMethod(1, 2);

        //     areas[2].anchoredPosition = pos;
        //     areas[2].sizeDelta = size;
        //     areas[2] = areas[3];

        //     ClicksCounter();
        // }
        // else
        // {
        //     Clicks--;
        //     pos = areas[2].anchoredPosition;
        //     size = areas[2].sizeDelta;
        //     areas[3] = areas[2];

        //     ArenesMethod(2, 1);

        //     ArenesMethod(1, 0);

        //     areas[0].anchoredPosition = pos;
        //     areas[0].sizeDelta = size;
        //     areas[0] = areas[3];

        //     ClicksCounter();
        // }

    }
}
