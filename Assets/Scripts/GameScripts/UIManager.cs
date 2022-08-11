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

    [Header("Tutorial")]
    [SerializeField] private Tutorial _tutorial;


    [SerializeField] private SceneLoadController sceneLoadController;
    [SerializeField] private RectTransform[] areas;

    // about chips
    [SerializeField] private GameObject _selectChipWindow;
    [SerializeField] private User user;

    private int sceneToLoad; // Индекс загрузки сцены
    private int tutorialScene = 2;

    private Vector2 pos; // 
    private Vector2 size; // 

    private bool _chipLoaded;
    private bool _loadingData;
    private bool _confirmChoose;
    private bool _selecting;


    private void Start()
    {
        sceneToLoad = 1;
    }

    public void PanelActivate(GameObject objectToClose) // 
    {
        user.SelectedChip = PlayerPrefs.GetInt("chipIndex");
        objectToClose.SetActive(!objectToClose.activeSelf);
    }

    public void QuitMethod() // 
    {
        Application.Quit();
    }

    public void PrepareLoadGame()
    {
        if (PlayerPrefs.GetString("GameMode") == "three")
        {
            _selectChipWindow.GetComponent<ShowingChipsController>().Selecting = true;
            _selectChipWindow.SetActive(true);
        }
        else
        {
            LoadGame();
        }
    }

    public async void LoadGame()
    {
        if (!_loadingData)
        {
            _loadingData = true;
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
                    if (!_tutorial._tutorial)
                    {
                        sceneLoadController.LoadGame(sceneToLoad);
                    }
                    else
                    {
                        _tutorial._tutorial = false;
                        user.Tutorial = true;
                        sceneLoadController.LoadGame(tutorialScene);
                    }
                }
                else
                {
                    // Reloggin
                    _authControllerObj.SetActive(true);
                    _selectAreaObj.SetActive(false);
                    _userData.ResetUser();

                _authControllerObj.GetComponent<AuthController>().Authorization(userWallet, true);
                }
            }
            catch
            {
            print("Error: " + response);
            }

            _loadingData = false;
        }
    }

    private class NFTs
    {
        public string contract { get; set; }
        public string tokenId { get; set; }
        public string uri { get; set; }
        public string balance { get; set; }
    }
}
