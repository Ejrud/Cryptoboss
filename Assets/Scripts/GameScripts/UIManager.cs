using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using NotificationSamples;

public class UIManager : MonoBehaviour
{
    [Header("Notifications")]
    [SerializeField] private GameNotificationsManager _gameNotificationsManager;
    [SerializeField] private string[] _notificationStr = {
        "The other players are waiting for you to play!",
        "You are close to next place on the leaders chart!",
        "You have untapped energy. Don't waste your time. Let's play!",
        "Your bossy bag get bored. Let's fill it up"
        };

    [Header("Relog links")]
    [SerializeField] private GameObject _authControllerObj;
    [SerializeField] private GameObject _selectAreaObj;
    [SerializeField] private UserData _userData; 


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

        GameNotificationChannel channel = new GameNotificationChannel("Cryptoboss", "Cryptoboss", "Just a notification");
        _gameNotificationsManager.Initialize(channel);
    }

    public void PanelActivate(GameObject objectToClose) // 
    {
        user.SelectedChipId = PlayerPrefs.GetInt("chipIndex");
        objectToClose.SetActive(!objectToClose.activeSelf);
    }

    public void QuitMethod() // 
    {
        CreateNotification();
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
                // chipFrame.GetComponent<ChipFrameData>().Init(user.ChipParam[i], true);
                chipFrame.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }

            chipLoaded = true;
        }
        
    }

    public async void LoadGame()
    {
        string chain = "polygon";
        string network = "mainnet"; // mainnet ropsten kovan rinkeby goerli
        string contract = "0x4fa6d1Fc702bD7f1607dfeE4206Db368995E1443"; // 0x4fa6d1Fc702bD7f1607dfeE4206Db368995E1443
        int first = 500;
        int skip = 0;
        string userWallet = user.Wallet;

        string response = await EVM.AllErc721(chain, network, userWallet, contract, first, skip);

        try
        {
            NFTs[] erc721s = JsonConvert.DeserializeObject<NFTs[]>(response);

            if (erc721s.Length == user.ChipParam.Count)
            {
                sceneLoadController.LoadGame(sceneToLoad);
            }
            else
            {
                // Reloggin
                _authControllerObj.SetActive(true);
                _selectAreaObj.SetActive(false);
                _userData.ResetUser();

               _authControllerObj.GetComponent<AuthController>().PrepareAuth(userWallet, true);
            }
        }
        catch
        {
           print("Error: " + response);
        }
    }

    public void CreateNotification()
    {
        string title = "Cryptoboss";
        string body = _notificationStr[0];
        DateTime time = DateTime.Now.AddSeconds(1f);

        IGameNotification notification = _gameNotificationsManager.CreateNotification();
        if (notification != null)
        {
            notification.Title = title;
            notification.Body = body;
            notification.DeliveryTime = time;
            _gameNotificationsManager.ScheduleNotification(notification);
        }
    }

    private void ArenesMethod(int x, int y) // 
    {
        areas[x].anchoredPosition = areas[y].anchoredPosition;
        areas[x].sizeDelta = areas[y].sizeDelta;
        areas[x] = areas[y];
    }

    private class NFTs
    {
        public string contract { get; set; }
        public string tokenId { get; set; }
        public string uri { get; set; }
        public string balance { get; set; }
    }
}
