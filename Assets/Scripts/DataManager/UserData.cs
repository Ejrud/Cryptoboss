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
                SetCard(user.chipDatas[i].ChipName, i); // Выдача карт фишке
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

            string newImgUri = "https://cryptoboss.win/game/back/images/" + tokenIds[i] + ".png"; // http://a0664627.xsph.ru/cryptoboss_back/images/ // https://cryptoboss.win/game/back/images/

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

    private void SetCard(string chipIndex, int massive_id)
    {
        List<CardData> listCards = new List<CardData>();
        Debug.Log("Присвоение карт");

        Debug.Log(user.cards[0].guid + "   " + chipIndex);

        for (int i = 0; i < user.cards.Count; i++)
        {
            if (user.cards[i].guid == chipIndex)
            {
                CardData cards = new CardData();
                cards.Guid = user.cards[i].card_id;
                cards.Type = user.cards[i].type;
                cards.Name = user.cards[i].name;
                cards.EnergyDamage = Convert.ToInt32(user.cards[i].loss_energy);
                cards.CapitalDamage = Convert.ToInt32(user.cards[i].loss_capital);
                cards.EnergyHealth = Convert.ToInt32(user.cards[i].heal_energy);
                cards.CapitalEarnings = Convert.ToInt32(user.cards[i].profit);
                cards.DamageResistance = Convert.ToInt32(user.cards[i].armor_of_loss);
                cards.CardCost = Convert.ToInt32(user.cards[i].energy_cost);
                cards.ChipId = user.cards[i].guid;
                cards.Description = user.cards[i].description;

                listCards.Add(cards);
            }
        }

        user.chipDatas[massive_id].CardDeck = new CardData[listCards.Count]; 

        for (int i = 0; i < listCards.Count; i++)
        {
            user.chipDatas[massive_id].CardDeck[i] = listCards[i];
        }
    }
}