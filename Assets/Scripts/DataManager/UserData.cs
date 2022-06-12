using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.IO;

public class UserData : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private SelectChip selectChip;
    [SerializeField] private User user;

    [Header("UI")]
    [SerializeField] private Text nameTxt;
    [SerializeField] private Text pointsTxt;
    [SerializeField] private Text moneyTxt;
    [SerializeField] private Text energyTxt;
    [SerializeField] private Text loadText;
    [SerializeField] private Image loadBar;

    [Header("Windows")]
    [SerializeField] private GameObject loadScreen;
    [SerializeField] private GameObject authWindow;

    [Header("Debug chips")]
    [SerializeField] private ChipData[] chips;

    // private List<DbCards> nullCards;
    private string pathToSave;
    private string saveFileName = "Data";

    private void Start()
    {
        loadScreen.SetActive(false);

        if (!user.Authorized)
        {
            ResetUser();
        }
        else
        {
            authWindow.SetActive(false);
            LoadNfts(user.nftTokens);
        }
    }

    private void LoadNfts(string[] tokenIds, float loadProgress = 0, int errorLoop = 0)
    {

        user.chipDatas = new ChipData[tokenIds.Length];
        user.nftTokens = tokenIds;
        LoadTextures(tokenIds, loadProgress, errorLoop);
        
    }

    private async void LoadTextures(string[] tokenIds, float loadProgress = 0, int errorLoop = 0)
    {
        string chain = "polygon";
        string network = "mainnet";
        string contract = "0x4fa6d1Fc702bD7f1607dfeE4206Db368995E1443";
        float MaxLoad = tokenIds.Length;
        float loadStep = 1 / MaxLoad;
        float currentLoad = loadProgress;
        loadBar.fillAmount = currentLoad;
        loadText.text = currentLoad * 100 + " %";

        loadScreen.SetActive(true);

        for (int i = errorLoop; i < tokenIds.Length; i++)
        {
            user.chipDatas[i] = chips[i]; // создание ScriptableObject тут можно присвоить новые карты
            user.chipDatas[i].Id = Convert.ToInt32(user.nftTokens[i]);
            user.chipDatas[i].ChipName = "CryptoBoss #" + user.chipDatas[i].Id;
            
            if(!user.Authorized)
            {
                SetCard(i); // Выдача карт фишке
            }

            // fetch uri from chain
            string uri = await ERC721.URI(chain, network, contract, tokenIds[i]); // change to ERC721

            UnityWebRequest webRequest = UnityWebRequest.Get(uri);
            await webRequest.SendWebRequest();
            if(webRequest.error != null)
            {
                Debug.Log("Repeat load");
                LoadTextures(tokenIds, currentLoad, i);
                return;
            }
            Response data = JsonUtility.FromJson<Response>(System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data));

            // parse json to get image uri
            string imageUri = data.image;

            string newImgUri = "http://a0664627.xsph.ru/cryptoboss_back/images/" + tokenIds[i] + ".png"; // http://a0664627.xsph.ru/cryptoboss_back/images/ // https://cryptoboss.win/game/back/images/

            // fetch image and display in game
            UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(newImgUri); // imageUri
            await textureRequest.SendWebRequest();
            if(textureRequest.error != null)
            {
                Debug.Log("Repeat load");
                LoadTextures(tokenIds, currentLoad, i);
                return;
            }

            Texture2D nft = ((DownloadHandlerTexture)textureRequest.downloadHandler).texture;

            // Сохранение текстуры локально
            user.chipDatas[i].ChipTexture = nft;

            currentLoad = UpdateLoading(currentLoad, loadStep);
            
            // Debug.Log("Load stage: " + currentLoad);
        }

        Debug.Log("Load complete");

        TextureData loadTextures = new TextureData();
        pathToSave = Path.Combine(Application.persistentDataPath, saveFileName);
        BinarySerializer.Serialize(pathToSave, loadTextures);

        loadScreen.SetActive(false);

        user.Authorized = true;

        UpdateUI();
    }
    public void SetUser(string userID, string userName, string email, string wallet, string score, string[] chipIds, List<DbCards> cards)
    {
        user.UserID = userID;
        user.UserName = userName;
        user.Email = email;
        user.Wallet = wallet;
        user.Score = score;
        user.cards = cards;

        PlayerPrefs.SetString("Wallet", user.Wallet);
        LoadNfts(chipIds);
    }

    // Сброс данных пользователя
    public void ResetUser()
    {
        user.UserID = "0";
        user.UserName = null;
        user.Email = null;
        user.Wallet = null;
        user.Score = null;
        user.chipDatas = new ChipData[0];
        user.nftTokens = new string[0];
        user.Authorized = false;
        // user.cards = nullCards;

        authWindow.SetActive(true);

        PlayerPrefs.SetString("Wallet", "");
    }

    public void ExitFromAccount()
    {
        ResetUser();
        SceneManager.LoadScene(0);
    }

    public void UpdateUI()
    {
        nameTxt.text = user.UserName;
        pointsTxt.text = user.Score;
        moneyTxt.text = "0";
        energyTxt.text = "10";
        selectChip.Init(user.chipDatas);
    }

    public bool IsAuthorized()
    {
        return user.Authorized;
    }

    public class Response 
    {
        public string image;
    }

    private float UpdateLoading(float currentLoad, float loadStep)
    {
        currentLoad += loadStep;
        loadBar.fillAmount = currentLoad; // Обновление загрузки фишек
        loadText.text = currentLoad * 100 + " %";

        return currentLoad;
    }

    private void SetCard(int chipIndex)
    {
        for (int i = 0; i < 10; i++)
        {
            int offset = chipIndex * 10;

            user.chipDatas[chipIndex].CardDeck[i].Name = user.cards[i+offset].name;
            user.chipDatas[chipIndex].CardDeck[i].Type = user.cards[i+offset].type;
            user.chipDatas[chipIndex].CardDeck[i].EnergyDamage = Convert.ToInt32(user.cards[i+offset].loss_energy);
            user.chipDatas[chipIndex].CardDeck[i].CapitalDamage = Convert.ToInt32(user.cards[i+offset].loss_capital);
            user.chipDatas[chipIndex].CardDeck[i].EnergyHealth = Convert.ToInt32(user.cards[i+offset].heal_energy);
            user.chipDatas[chipIndex].CardDeck[i].CapitalEarnings = Convert.ToInt32(user.cards[i+offset].profit);
            user.chipDatas[chipIndex].CardDeck[i].DamageResistance = Convert.ToInt32(user.cards[i+offset].armor_of_loss);
            user.chipDatas[chipIndex].CardDeck[i].CardCost = Convert.ToInt32(user.cards[i+offset].energy_cost);
        }
    }
}