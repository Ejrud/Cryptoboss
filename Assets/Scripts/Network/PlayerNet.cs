using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Mirror;
using System;
using TMPro;

public class PlayerNet : NetworkBehaviour
{
    [Header("Player parameters")]
    public string Wallet;
    public string UserName;
    public string GameMode;
    public int ChipId;
    public CardData[] CardCollection;
    public CardData[] HandCards;
    public CardData[] PreviousCards;
    public CardData PreviousCard;
    public CardData RivalCard;
    public int HedgeFundCount = 0;

    public PlayerNet Friend;
    public EmotionNet emotions;

    public Impact PlayerImpact = new Impact();
    public bool TurnAround;
    public bool Scam;
    public bool ToTheMoon;
    public bool Pump;

    public int MaxHealth;
    public int Capital;                   // Hp
    public float Morale;                    // Energy
    public float OriginMorale;
    public int EnemyCapital;
    public float EnemyEnergy;
    public float MaxEnergy;
    public bool Win = false;
    public bool MyTurn = false;
    public bool CardSelected;
    public bool ChipReceived;
    public bool FirstStart;
    public bool Understudy;
    public int SelectedCardId;
    public int UsedCount;
    
    private string seUrl = "https://cryptoboss.win/game/back/"; // http://a0664627.xsph.ru/cryptoboss_back/  // https://cryptoboss.win/game/back/

    private int currentRating;
    private float bossyReward;
    private int ratingReward;
    private int ratingLose = 8;

    #region UI elements
    [Header("player UI")]
    [SerializeField] private ChipRepresentation chipRepresentation;
    [SerializeField] private TurnVisualize turnVisualize;
    [SerializeField] private Text timerText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text energyText;
    [SerializeField] private TMP_Text exitWindowText;
    [SerializeField] private Text bossyText;
    [SerializeField] private Text raitingText;
    [SerializeField] private Image healthImage;
    [SerializeField] private Image energyImage;
    [SerializeField] private RawImage ChipImage;        // Используется для настройки цвета
    [SerializeField] private RawImage FriendChipImage;        // Используется для настройки цвета
    [SerializeField] private RawImage RivalChipImage;   // Используется для настройки цвета
    [SerializeField] private RawImage FriendRivalChipImage;   // Используется для настройки цвета
    [SerializeField] private GameObject winContainer;
    [SerializeField] private Text awaitPlayerTxt;


    [SerializeField] private GameObject playersWaitingObj;
    [SerializeField] private GameObject representationScreen;
    [SerializeField] private GameObject backToLobbyWindow;
    [SerializeField] private GameObject LoadRewardWindow;

    [Header("Enemy UI")]
    [SerializeField] private Image enemyHealthImage;
    [SerializeField] private Image enemyEnergyImage;
    [SerializeField] private Text enemyhealthText;
    [SerializeField] private Text enemyEnergyText;

    [Header("Gameplay")]
    [SerializeField] private CardManager cardManager;
    [SerializeField] private int menuSceneIndex = 0;
    [SerializeField] private User user;
    [SerializeField] private RawImage[] userChipImages_1;  // Изображения на экране презентации, и в игровом процессе
    [SerializeField] private RawImage[] userChipImages_2; // Изображения на экране презентации, и в игровом процессе
    [SerializeField] private RawImage[] userChipImages_3; // Изображения на экране презентации, и в игровом процессе
    [SerializeField] private RawImage[] rivalChipImages_1; // Изображения на экране презентации, и в игровом процессе
    [SerializeField] private RawImage[] rivalChipImages_2; // Изображения на экране презентации, и в игровом процессе
    [SerializeField] private RawImage[] rivalChipImages_3; // Изображения на экране презентации, и в игровом процессе
    [SerializeField] private Texture lastTexture;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private EmotionController emotionController;
    private Texture2D[] rivalChipTexture;
    
    private NetworkController controller;
    private List<BossyRewardParams> bossyParam = new List<BossyRewardParams>();

    #endregion

    // 
    private void Start()
    {
        audioSource.volume = PlayerPrefs.GetFloat("musicVolume");
        audioSource.mute = user.Mute;

        playersWaitingObj.SetActive(true);
        representationScreen.SetActive(false);
        LoadRewardWindow.SetActive(false);

        GameObject netObject = FindObjectOfType<NetworkManager>().gameObject;
        
        gameObject.SetActive(false);

        FirstStart = true;

        if (isServer)
        {
            controller = FindObjectOfType<NetworkController>();
        }

        if (hasAuthority)
        {
            gameObject.SetActive(true);

            // Wallet = PlayerPrefs.GetString("Wallet");
            ChipId = PlayerPrefs.GetInt("chipId");
            GameMode = PlayerPrefs.GetString("GameMode");
            UserName = user.UserName;
            Debug.Log(UserName);
            
            CmdSendWalletAndId(Wallet, ChipId, ChipReceived, UserName);
            CalculateResults(); // Локально у игроков висчитывать результаты игры

            Texture chipTexture = lastTexture;

            Debug.Log(GameMode);

            if (GameMode == "one")
            {
                PrepareChip(rivalChipImages_2, 0, false); // где bool просто скрыть объект. цифра - индекс текстуры 
                PrepareChip(rivalChipImages_3, 0, false);
                PrepareChip(userChipImages_2, 0, false);
                PrepareChip(userChipImages_3, 0, false);
            }
            else if (GameMode == "two") // Скрыть лишние фишки и показать 2 текущей фишки и 2 фишки соперников 
            {
                PrepareChip(rivalChipImages_2, 0, false); // где bool просто скрыть объект. цифра - индекс текстуры 
                PrepareChip(rivalChipImages_3, 0, false);
                PrepareChip(userChipImages_2, 0, false);
                PrepareChip(userChipImages_3, 0, false);
                
            }
            else if (GameMode == "three") // Показать 3 фишки соперника (т.к. 3 на 3 играют 2 игрока, то фишки игрока были изначально загружены при авторизации)
            {
                
            }

            // 
            for (int i = 0; i < user.ChipParam.Count; i++)
            {

                if (ChipId == user.ChipParam[i].Id)
                {
                    chipTexture = user.ChipParam[i].ChipTexture;

                    foreach(RawImage chipImage in userChipImages_1) //
                    {
                        chipImage.texture = chipTexture;
                    }
                    Debug.Log("Chip loaded = " + ChipId);
                    return;
                }
                else    // 
                {
                    Debug.Log("ChipId not found. prefab id = " +  ChipId);

                    chipTexture = user.ChipParam[UnityEngine.Random.Range(0, user.ChipParam.Count - 1)].ChipTexture;

                    foreach(RawImage chipImage in userChipImages_1) //
                    {
                        chipImage.texture = chipTexture;
                    }
                }
            }
        }
    }

    // 
    public void UpdatePlayerCharacteristic(int Capital, float Morale, int EnemyCapitale, float EnemyEnergy, float maxEnergy)
    {
        this.Capital = Capital;
        this.Morale = Morale;
        this.EnemyCapital = EnemyCapitale;
        this.EnemyEnergy = EnemyEnergy;
        this.OriginMorale = Morale;
        this.MaxEnergy = maxEnergy;

        UpdateClientParameters(this.Capital, this.Morale, this.EnemyCapital, this.EnemyEnergy, this.MaxEnergy, this.MaxHealth);
    }

    // 
    public void UpdateRoundCards(CardData[] hand, bool reconnect = false)
    {
        HandCards = hand;
        CardSelected = false;

        if(!reconnect)
        {
            foreach (CardData card in HandCards)
            {
                card.Used = false;
            }

            UsedCount = hand.Length;
        }
        
        SyncRoundCards(HandCards, UsedCount);
    }
    

    // 
    public void UpdateUI()
    {
        float floatCapital = Convert.ToSingle(Capital);
        healthImage.fillAmount = floatCapital / MaxHealth;
        float floatMorale = Convert.ToSingle(Morale);
        energyImage.fillAmount = floatMorale / MaxEnergy;

        // Debug.Log("Capital = " + Capital + " healthImage.fillAmount = " + Capital * 0.1f);
        healthText.text = Capital.ToString();
        energyText.text = Morale.ToString();

        float floatCapEnemy = Convert.ToSingle(EnemyCapital);
        enemyHealthImage.fillAmount = floatCapEnemy / MaxHealth;
        float floatEnergy = Convert.ToSingle(EnemyEnergy);
        
        enemyEnergyImage.fillAmount = floatEnergy / MaxEnergy; // / maxEnergy
        enemyhealthText.text = EnemyCapital.ToString();
        enemyEnergyText.text = EnemyEnergy.ToString();
    }

    public void ExitToMainMenu()
    {
        NetworkManager.singleton.StopClient();
        GameObject netObj = FindObjectOfType<NetworkManager>().gameObject;
        NetworkController controller = netObj.GetComponent<NetworkController>();
        controller.HideExitWindow();
        Destroy(netObj);
        SceneManager.LoadScene(menuSceneIndex);
    }
    [Command]
    public async void CmdSendGameMode(string gameMode, string wallet, int chipId)
    {
        bool hasEnergy = false;
        WWWForm form = new WWWForm();
        form.AddField("Guid", $"CryptoBoss #{chipId}");

        using (UnityWebRequest request = UnityWebRequest.Post(seUrl + "checkEnergy.php", form))
        {
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                if (request.downloadHandler.text == "true")
                {
                    hasEnergy = true;
                }
            }
            else
            {
                hasEnergy = false;
            }
        }

        GameMode = gameMode;
        Wallet = wallet;
        this.ChipId = chipId;
        // Debug.Log(GameMode);
        FindObjectOfType<NetworkController>().SetDistribution(this, hasEnergy); // cringe
    }

    [Command] // 
    public void CmdSendWalletAndId(string wallet, int chipId, bool received, string name)
    {
        Wallet = wallet;
        ChipId = chipId;
        ChipReceived = received;
        UserName = name;
    }

    [Command]
    public void PlayerStatus(bool cardsSelected, int selectedCardId)
    {
        CardSelected = cardsSelected;
        SelectedCardId = selectedCardId;
        HandCards[SelectedCardId].Used = true;
        UsedCount--;

        PlayerImpact.CapitalDamage = HandCards[selectedCardId].CapitalDamage;
        PlayerImpact.CapitalHealth = HandCards[selectedCardId].CapitalEarnings;
        PlayerImpact.JokerName = HandCards[selectedCardId].Name;
    }

    [Command]
    public void CmdSendEmotion(int index)
    {
        emotions.PlayEmotion(index);
    }

    [ClientRpc]
    public void RpcRecieveEmotion(int index, string gameMode)
    {
        if (hasAuthority)
        {
            emotionController.RecieveEmotion(index, gameMode);
        }
    }

    [ClientRpc]
    public void SetAudit(CardData[] handCards)
    {
        if (hasAuthority)
        {
            // 
            cardManager.AuditRivalCards(handCards);
        }
    }

    [ClientRpc]
    public void ResetAudit()
    {
        if (hasAuthority)
        {
            // 
            cardManager.DisableAudit();
        }
    }

    [ClientRpc]
    public void UpdateUICards(CardData[] cardData)
    {
        HandCards = cardData;
        cardManager.LocalUpdate();
    }

    [ClientRpc]
    public void SetTurn(bool turn)
    {
        if (hasAuthority)
        {
            MyTurn = turn;

            cardManager.SetCardPositions(MyTurn);

            if (MyTurn)
            {
                ChipImage.color = Color.white;
                RivalChipImage.color = Color.gray;

                if (Morale >= 1)
                    turnVisualize.SetTurn(); // Cringe

                if (GameMode == "two")
                {
                    FriendChipImage.color = Color.gray;
                    FriendRivalChipImage.color = Color.gray;
                }
            }
            else
            {
                if (GameMode == "two")
                {
                    if (Friend.MyTurn)
                    {
                        FriendChipImage.color = Color.white;
                        RivalChipImage.color = Color.gray;
                        FriendRivalChipImage.color = Color.gray;
                    }
                    else
                    {
                        FriendChipImage.color = Color.gray;
                        RivalChipImage.color = Color.white;
                        FriendRivalChipImage.color = Color.white;
                    }
                }
                else
                {
                    RivalChipImage.color = Color.white;
                }

                ChipImage.color = Color.gray;
            }
        }
    }

    [ClientRpc]
    public void RecieveRivalCards(int cardId, CardData rivalCard)
    {
        if (hasAuthority) // 
        {
            RivalCard = rivalCard;
            cardManager.RivalCardSelect(cardId, RivalCard);
        }
    }

    [ClientRpc]
    public void SyncRoundCards(CardData[] roundCards, int usedCount)
    {
        if (hasAuthority)
        {
            Debug.Log("Local player prepared");

            UsedCount = usedCount;
            HandCards = roundCards;
            CardSelected = false;
            cardManager.ReturnCards();
        }
    }

    [ClientRpc]
    public void SetTimerText(string time)
    {
        timerText.text = time;
        awaitPlayerTxt.text = time;
        UpdateUI();
    }

    [ClientRpc]
    public void CloseWindowWaitingForPlayers()
    {
        playersWaitingObj.SetActive(false);
    }

    [ClientRpc]
    public void RepresentationWindow(bool view)
    {
        representationScreen.SetActive(view);
        backToLobbyWindow.SetActive(false);

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    [ClientRpc]
    public void UpdateClientParameters(int Capital, float Morale, int EnemyCapital, float EnemyEnergy, float maxEnergy, int MaxCapital)
    {
        this.Morale = Morale;
        this.Capital = Capital;
        this.EnemyCapital = EnemyCapital;
        this.EnemyEnergy = EnemyEnergy;
        this.OriginMorale = Morale;
        this.MaxEnergy = maxEnergy;
        this.MaxHealth = MaxCapital;

        UpdateUI();
    }

    [ClientRpc]
    public void StopGame(string text, string charIncrement, float bossy, string raiting, bool disconnected, bool win)
    {
        backToLobbyWindow.SetActive(true);

        if (disconnected) // Если соперник "случайно" отключился, то ожидать 60 секунд или выйти и не получмить награду
        {
            exitWindowText.text = text;
            winContainer.SetActive(false);
            awaitPlayerTxt.gameObject.SetActive(true);
        }
        else
        {
            if (win)
            {
                bossy = (float)Math.Round(bossyReward, 2);
                raiting = ratingReward.ToString();
            }
            else
            {
                bossy = 0;
                raiting = ratingLose.ToString();
            }

            awaitPlayerTxt.gameObject.SetActive(false);
            winContainer.SetActive(true);
            exitWindowText.text = text;
            bossyText.text = charIncrement + bossy;
            raitingText.text = charIncrement + raiting;
        }
    }

    [ClientRpc]
    public void LoadReward()
    {
        if(hasAuthority)
        {
            LoadRewardWindow.SetActive(true);
        }
    }

    [ClientRpc]
    public async void LoadRivalChip(int[] rivalChipId, string gameMode, string[] names, string[] chipNames) // 
    {
        if(hasAuthority)
        {
            // names[0] = UserName;
            Debug.Log("Load textures...");
            rivalChipTexture = new Texture2D[rivalChipId.Length];
            // В зависимости от режима будут загружаться фишки других игроков последний индекс массива - фишка сокомандника
            for (int i = 0; i < rivalChipId.Length; i++)
            {
                string newImgUri = seUrl + "images/" + rivalChipId[i] + ".png";

                // fetch image and display in game
                UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(newImgUri); // imageUri
                await textureRequest.SendWebRequest();
                rivalChipTexture[i] = ((DownloadHandlerTexture)textureRequest.downloadHandler).texture;

                if(textureRequest.error != null) //
                {
                    LoadRivalChip(rivalChipId, gameMode, names, chipNames);
                    return;
                }
                Debug.Log("Load complete");
            }

            ChipReceived = true;
            CmdSendWalletAndId(Wallet, ChipId, ChipReceived, UserName);

            if (gameMode == "one")
            {
                PrepareChip(rivalChipImages_2, 0, false); // где bool просто скрыть объект. цифра - индекс текстуры 
                PrepareChip(rivalChipImages_3, 0, false);
                PrepareChip(userChipImages_2, 0, false);
                PrepareChip(userChipImages_3, 0, false);
                
                PrepareChip(rivalChipImages_1, 0, true);
                PrepareChip(userChipImages_1, 0, true, true); // Если текущий игрок, то игнорировать присвоение текстуры 
            }
            else if (gameMode == "two") // Скрыть лишние фишки и показать 2 текущей фишки и 2 фишки соперников 
            {
                PrepareChip(rivalChipImages_3, 0, false);
                PrepareChip(userChipImages_3, 0, false);

                PrepareChip(rivalChipImages_1, 0, true);
                PrepareChip(rivalChipImages_2, 1, true);
                PrepareChip(userChipImages_2, 2, true);
            }
            else if (gameMode == "three") // Показать 3 фишки соперника (т.к. 3 на 3 играют 2 игрока, то фишки игрока были изначально загружены при авторизации)
            {
                PrepareChip(rivalChipImages_1, 0, true);
                PrepareChip(rivalChipImages_2, 1, true);
                PrepareChip(rivalChipImages_3, 2, true);
            }
            else
            {
                StopGame("Game mode not selected", "", 0f, "0", false, false);
            }

            chipRepresentation.SetUpWindows(names, chipNames, gameMode);
        }
    }

    private void PrepareChip(RawImage[] playerImages, int index, bool active, bool currentUser = false)
    {
        if (active)
        {
            foreach (RawImage image in playerImages)
            {
                image.gameObject.SetActive(active);
                if (!currentUser)
                {
                    image.texture = rivalChipTexture[index];
                }
            }
        }
        else
        {
            foreach (RawImage image in playerImages)
            {
                image.gameObject.SetActive(active);
            }
        }
    }
    

    [ClientRpc]
    public void GetGameMode()
    {
        if (hasAuthority)
        {
            string gameMode = PlayerPrefs.GetString("GameMode");
            Wallet = user.Wallet;
            int chipId = PlayerPrefs.GetInt("chipId");
            GameMode = gameMode;
            CmdSendGameMode(gameMode, Wallet, chipId);
        }
    }

    private async void CalculateResults() 
    {
        if (hasAuthority)
        {
            // Поиск текущего рейтинга, bossy и награда рейтинга и bossy за игру
            WWWForm form = new WWWForm();
            form.AddField("ChipGuid", "CryptoBoss #" + ChipId);

            UnityWebRequest webRequest = UnityWebRequest.Post(seUrl + "/get_chipData.php", form);
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                List<ChipRating> chipRatings = JsonConvert.DeserializeObject<List<ChipRating>>(webRequest.downloadHandler.text);
                Debug.Log(webRequest.downloadHandler.text);
                currentRating = Convert.ToInt32(chipRatings[0].rating); // текущий рейтинг
            }

            form = new WWWForm();
            form.AddField("Mode", PlayerPrefs.GetString("GameMode"));

            webRequest = UnityWebRequest.Post(seUrl + "/get_ratingCoefficient.php", form);

            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                ratingReward = Convert.ToInt32(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.Log(webRequest.error);
            }

            webRequest = UnityWebRequest.Post(seUrl + "/get_bossyParams.php", form);
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                bossyParam = JsonConvert.DeserializeObject<List<BossyRewardParams>>(webRequest.downloadHandler.text); 
            }

            // Предполагаемая награза в зависимости от результата игры
            float winRating = ratingReward;
            float division = 1000;
            float ratingPlus = 1;

            float cof = (currentRating + winRating) / (division + ratingPlus);    //    0.35 = (344 + 10) / 1001
            // decimal bossyDef = Convert.ToDecimal(bossyParam[0].bossy_count);

            bossyReward = 10 + (10 * cof); // (float)bossyDef   13.5 = 10 + (10 * x)  =   3.5 = 10 * x   0.35
        }   
    }

    private void OnApplicationQuit()
    {
        if (hasAuthority)
        {
            user.Authorized = false;
        }
       
    } 

    public class Response 
    {
        public string image;
    }

    public class ChipRating
    {
        public string rating { get; set; }
    }

    public class DbRating
    {
        public string rating_bd { get; set; }
    }

    public class BossyRewardParams
    {
        public string bossy_count { get; set; }
        public string rating_div { get; set; }
        public string rating_plus { get; set; }
    }
    
    // Сохранение действий прошлой карты 
    public class Impact
    {
        public string JokerName;
        public int CapitalDamage;
        public int CapitalHealth;
        public int Energy;
        
        public bool Increased;
        public bool Decreased;
    }
}