using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.IO;
using Newtonsoft.Json;

public class UserData : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Tutorial _tutorial;
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
    private string seUrl = "https://cryptoboss.win/game/back/"; // http://a0664627.xsph.ru/cryptoboss_back/ // https://cryptoboss.win/game/back/

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
        
        user.nftTokens = tokenIds;
        LoadData(tokenIds, loadProgress, errorLoop);
    }

    private async void LoadData(string[] tokenIds, float loadProgress = 0, int errorLoop = 0)
    {
        float MaxLoad = tokenIds.Length;
        float loadStep = 1 / MaxLoad;
        float currentLoad = loadProgress;
        loadBar.fillAmount = currentLoad;
        int loadRounded = (int)currentLoad * 100;
        loadText.text = (int)(currentLoad * 100) + " %";
        loadScreen.SetActive(true);

        string uri = $"https://cryptoboss.win/ajax/models/comments/customizers/get_user_balance_ixdkznne7?address={user.Wallet}";

        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        
        // Request and wait for the desired page.
        await webRequest.SendWebRequest();

        string[] pages = uri.Split('/');
        int page = pages.Length - 1;

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            UserBalance Balance = JsonConvert.DeserializeObject<UserBalance>(webRequest.downloadHandler.text);
            user.Balance = Balance.balance;
        }

        for (int i = errorLoop; i < tokenIds.Length; i++)
        {
            ChipParameters chip = new ChipParameters();

            if (user.ChipParam.Count != tokenIds.Length)
            {
                user.ChipParam.Add(chip);
            }

            user.ChipParam[i].Id = Convert.ToInt32(user.nftTokens[i]);
            user.ChipParam[i].ChipName = "CryptoBoss #" + user.ChipParam[i].Id;
            
            SetCard(user.ChipParam[i].ChipName, i); //

            // Debug.Log("CryptoBoss #" + tokenIds[i]);

            // Загрузка данных фишки

            WWWForm form = new WWWForm();
            form.AddField("ChipGuid", user.ChipParam[i].ChipName);
            UnityWebRequest www = UnityWebRequest.Post(seUrl + "get_chipData.php", form);
            await www.SendWebRequest();
            List<ChipParam> chipParam = JsonConvert.DeserializeObject<List<ChipParam>>(www.downloadHandler.text);
            user.ChipParam[i].Capital = chipParam[0].capital_current;
            user.ChipParam[i].Morale = chipParam[0].energy_current;
            user.ChipParam[i].Rating = chipParam[0].rating;
            user.ChipParam[i].Description = chipParam[0].description;
            user.ChipParam[i].Species = chipParam[0].species;
            user.ChipParam[i].Role = chipParam[0].role;

            // Debug.Log(www.downloadHandler.text);

            if(!user.Authorized)
            {
                string newImgUri = seUrl + "images/" + tokenIds[i] + ".png"; // 
                // fetch image and display in game
                UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(newImgUri); // imageUri
                await textureRequest.SendWebRequest();
                if(textureRequest.error != null)
                {
                    Debug.Log(textureRequest.error + " Repeat load image");
                    LoadData(tokenIds, currentLoad, i);
                    return;
                }

                Texture2D nft = ((DownloadHandlerTexture)textureRequest.downloadHandler).texture;

                // ���������� �������� ��������
                user.ChipParam[i].ChipTexture = nft;
            }

            currentLoad = UpdateLoading(currentLoad, loadStep);
        }

        Debug.Log("Load complete");

        TextureData loadTextures = new TextureData();
        pathToSave = Path.Combine(Application.persistentDataPath, saveFileName);
        BinarySerializer.Serialize(pathToSave, loadTextures);

        loadScreen.SetActive(false);

        user.Authorized = true;

        _tutorial.gameObject.SetActive(true);
        _tutorial.StartTutorial(user.UserName);

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

    // ����� ������ ������������
    public void ResetUser()
    {
        user.UserID = "0";
        user.UserName = null;
        user.Email = null;
        user.Wallet = null;
        user.Score = null;
        user.ChipParam = new List<ChipParameters>();
        user.nftTokens = new string[0];
        user.Authorized = false;
        user.Balance = "";
        user.Mute = false;
        // user.cards = nullCards;

        authWindow.SetActive(true);

        PlayerPrefs.SetString("Wallet", "");
    }

    public void ExitFromAccount()
    {
        ResetUser();

        #if UNITY_ANDROID
        user.Authorized = false;
        Application.Quit();
        #endif
        #if UNITY_WEBGL
        user.Authorized = false;
        SceneManager.LoadScene(0);
        #endif
        #if UNITY_EDITOR
        user.Authorized = false;
        SceneManager.LoadScene(0);
        #endif
    }

    public void UpdateUI()
    {
        nameTxt.text = user.UserName;
        pointsTxt.text = user.Score;
        moneyTxt.text = user.Balance;
        energyTxt.text = "10";
        selectChip.Init(user.ChipParam);
    }

    public bool IsAuthorized()
    {
        return user.Authorized;
    }

    public class Response 
    {
        public string image;
    }

    public class ChipParam
    {
        public string capital_current { get; set; }
        public string energy_current { get; set; }
        public string description { get; set; }
        public string rating { get; set; }
        public string species { get; set; }
        public string role { get; set; }
    }

    private float UpdateLoading(float currentLoad, float loadStep)
    {
        currentLoad += loadStep;
        loadBar.fillAmount = currentLoad; // ���������� �������� �����
        loadText.text = (int)(currentLoad * 100) + " %";

        return currentLoad;
    }

    private void SetCard(string chipIndex, int massive_id)
    {
        List<CardData> listCards = new List<CardData>();
        // Debug.Log("���������� ����");

        // Debug.Log(user.cards[0].guid + "   " + chipIndex);

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

        user.ChipParam[massive_id].CardDeck = new CardData[listCards.Count]; 

        for (int i = 0; i < listCards.Count; i++)
        {
            user.ChipParam[massive_id].CardDeck[i] = listCards[i];
        }
    }

    private void OnApplicationQuit()
    {
        user.Authorized = false;
    }

    public class UserBalance
    {
        public string balance { get; set;}
    }
}