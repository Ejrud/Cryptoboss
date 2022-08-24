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
    public int[] ChipId;
    public CardData[] CardCollection;
    public CardData[] HandCards;
    public CardData[] PreviousCards;
    public CardData PreviousCard;
    public CardData RivalCard;
    public int HedgeFundCount = 0;

    public PlayerNet Friend;
    public int FriendIndex;
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
    [SyncVar]
    public bool MyTurn = false;
    public bool CardSelected;
    public bool ChipReceived;
    public bool FirstStart;
    public bool Understudy;
    public int SelectedCardId;
    public int UsedCount;
    
    private string seUrl = "https://cryptoboss.win/game/back/"; // http://a0664627.xsph.ru/cryptoboss_back/  // https://cryptoboss.win/game/back/
    private int currentRating;
    public float BossyReward;
    public int RatingReward;
    public int RatingLose = 8;

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
    [SerializeField] private CardSecondPlayerManager cardSecondPlayerManager;
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
    private Dictionary<int, RawImage> rivalsRawImage = new Dictionary<int, RawImage>();
    private NetworkController controller;
    private List<BossyRewardParams> bossyParam = new List<BossyRewardParams>();
    private bool _frinedTurn = false;

    #endregion

    // 
    private void Start()
    {
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
            audioSource.volume = PlayerPrefs.GetFloat("musicVolume");
            audioSource.mute = user.Mute;

            gameObject.SetActive(true);
            GameMode = PlayerPrefs.GetString("GameMode");

            if (GameMode == "three")
            {
                ChipId = new int[3];
                ChipId[0] = user.chipGuid_1;  ChipId[1] = user.chipGuid_2;  ChipId[2] = user.chipGuid_3;  
            }
            else
            {
                ChipId = new int[1];
                ChipId[0] = user.chipGuid_1;
            }

            // Wallet = PlayerPrefs.GetString("Wallet");
            UserName = user.UserName;
            
            CmdSendWalletAndId(Wallet, ChipId, ChipReceived, UserName);
            CalculateResults(); // Локально у игроков висчитывать результаты игры

            Texture chipTexture = lastTexture;

            Debug.Log(GameMode);

            if (GameMode == "one")
            {
                PrepareChip(userChipImages_1, 0, true, true);
                PrepareChip(rivalChipImages_2, 0, false); // где bool просто скрыть объект. цифра - индекс текстуры 
                PrepareChip(rivalChipImages_3, 0, false);
                PrepareChip(userChipImages_2, 0, false);
                PrepareChip(userChipImages_3, 0, false);
            }
            else if (GameMode == "two") // Скрыть лишние фишки и показать 2 текущей фишки и 2 фишки соперников 
            {
                PrepareChip(userChipImages_1, 0, true, true);
                PrepareChip(rivalChipImages_2, 0, false); // где bool просто скрыть объект. цифра - индекс текстуры 
                PrepareChip(rivalChipImages_3, 0, false);
                PrepareChip(userChipImages_2, 0, false);
                PrepareChip(userChipImages_3, 0, false);
                
            }
            else if (GameMode == "three") // Показать 3 фишки соперника (т.к. 3 на 3 играют 2 игрока, то фишки игрока были изначально загружены при авторизации)
            {
                PrepareChip(userChipImages_1, 0, true, true);
                PrepareChip(userChipImages_2, 1, true, true);
                PrepareChip(userChipImages_3, 2, true, true);
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

        if (GameMode == "two" && Friend != null)
        {
            Friend.Capital = Capital;
            Friend.Morale = Morale;
            Friend.EnemyCapital = EnemyCapitale;
            Friend.EnemyEnergy = EnemyEnergy;
            Friend.OriginMorale = Morale;
            Friend.MaxEnergy = maxEnergy;

            Friend.UpdateClientParameters(Capital, Morale, EnemyCapital, EnemyEnergy, MaxEnergy, MaxHealth);
        }

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
    public async void CmdSendGameMode(string gameMode, string wallet, int[] chipId)
    {
        bool hasEnergy = false;

        for (int i = 0; i < chipId.Length; i++)
        {
            WWWForm form = new WWWForm();
            form.AddField("Guid", $"CryptoBoss #{chipId[i]}");

            using (UnityWebRequest request = UnityWebRequest.Post(seUrl + "checkEnergy.php", form))
            {
                await request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (request.downloadHandler.text == "true")
                    {
                        Debug.Log(request.downloadHandler.text);
                        hasEnergy = true;
                    }
                    else
                    {
                        hasEnergy = false;
                        break;
                    }
                }
                else
                {
                    hasEnergy = false;
                    break;
                }
            }

            if (!hasEnergy) break;
        }

        GameMode = gameMode;
        Wallet = wallet;
        this.ChipId[0] = chipId[0];
        // Debug.Log(GameMode);
        FindObjectOfType<NetworkController>().SetDistribution(this, hasEnergy); // cringe
        Debug.Log("energy " + hasEnergy);
    }

    [Command] // 
    public void CmdSendWalletAndId(string wallet, int[] chipId, bool received, string name)
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
    public void SetTurn(bool turn, bool friendTurn, int queueIndex, CardData[] cards)
    {
        if (hasAuthority)
        {
            MyTurn = turn;

            cardManager.SetCardPositions(MyTurn, cards, friendTurn);

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
                if (GameMode == "three")
                {
                    userChipImages_2[2].color = Color.white;
                    userChipImages_3[2].color = Color.white;

                    rivalChipImages_1[1].color = Color.gray;
                    rivalChipImages_2[1].color = Color.gray;
                    rivalChipImages_3[1].color = Color.gray;
                }
            }
            else
            {
                if (GameMode == "two")
                {
                    if (friendTurn)
                    {
                        _frinedTurn = true;
                        FriendChipImage.color = Color.white;
                        RivalChipImage.color = Color.gray;
                        FriendRivalChipImage.color = Color.gray;
                    }
                    else
                    {
                        _frinedTurn = false;
                        FriendChipImage.color = Color.gray;
                        RivalChipImage.color = Color.gray;
                        FriendRivalChipImage.color = Color.gray;

                        if (rivalsRawImage.Count != 0)
                            rivalsRawImage[queueIndex].color = Color.white; // Помечать фишку соперника
                    }
                }
                else if (GameMode == "three")
                {
                    userChipImages_2[2].color = Color.gray;
                    userChipImages_3[2].color = Color.gray;

                    rivalChipImages_1[1].color = Color.white;
                    rivalChipImages_2[1].color = Color.white;
                    rivalChipImages_3[1].color = Color.white;
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
            cardManager.RivalCardSelect(cardId, RivalCard, _frinedTurn);
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
    public void wet(GameObject friend)
    {
        Friend = friend.GetComponent<PlayerNet>();
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
                bossy = (float)Math.Round(BossyReward, 2);
                raiting = RatingReward.ToString();
            }
            else
            {
                bossy = 0;
                raiting = RatingLose.ToString();
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
    public async void LoadRivalChip(int[] rivalChipId, string gameMode, string[] names, string[] chipNames, int[] chipQueueIndex) // 
    {
        if(hasAuthority)
        {
            rivalsRawImage = new Dictionary<int, RawImage>();
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
                    LoadRivalChip(rivalChipId, gameMode, names, chipNames, chipQueueIndex);
                    return;
                }
                // Debug.Log("Load complete");

                if (i == 0)
                    rivalsRawImage.Add(chipQueueIndex[i], RivalChipImage);
                else if (i == 1)
                    rivalsRawImage.Add(chipQueueIndex[i], rivalChipImages_2[1]);
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
                else
                {
                    for (int i = 0; i < user.ChipParam.Count; i++)
                    {

                        if (ChipId[index] == user.ChipParam[i].Id)
                        {
                            image.texture = user.ChipParam[i].ChipTexture;
                            break;
                        }
                    }
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
            int[] chipIds;
            Wallet = user.Wallet;

            if (gameMode == "three")
            {
                chipIds = new int[] { user.chipGuid_1, user.chipGuid_2, user.chipGuid_3 };
            }
            else
            {
                chipIds = new int[] { user.chipGuid_1 };
            }
             
            GameMode = gameMode;
            CmdSendGameMode(gameMode, Wallet, chipIds);
        }
    }

    // [ClientRpc]
    // public void UpdateFriendships(CardData[] playerData, CardData[] rivalData, bool myTurn, bool friendTurn, bool rivalFriendTurn)
    // {
    //     if (hasAuthority)
    //         cardManager.UpdateFriendships(playerData, rivalData, myTurn, friendTurn, rivalFriendTurn);
    // }

    private async void CalculateResults() 
    {
        // Поиск текущего рейтинга, bossy и награда рейтинга и bossy за игру
        WWWForm form = new WWWForm();
        UnityWebRequest webRequest;

        for (int i = 0; i < ChipId.Length; i++)
        {
            form.AddField("ChipGuid", "CryptoBoss #" + ChipId[i]);

            webRequest = UnityWebRequest.Post(seUrl + "/get_chipData.php", form);
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                List<ChipRating> chipRatings = JsonConvert.DeserializeObject<List<ChipRating>>(webRequest.downloadHandler.text);
                Debug.Log(webRequest.downloadHandler.text);
                currentRating = Convert.ToInt32(chipRatings[0].rating); // текущий рейтинг
            }
        }

        form = new WWWForm();
        form.AddField("Mode", GameMode);

        webRequest = UnityWebRequest.Post(seUrl + "/get_ratingCoefficient.php", form);

        await webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            RatingReward = Convert.ToInt32(webRequest.downloadHandler.text);
            Debug.Log(RatingReward);
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

        // Предполагаемая награда в зависимости от результата игры
        for (int i = 0; i < ChipId.Length; i++)
        {
            float division = 1000;
            float ratingPlus = 1;
            float cof = (currentRating + RatingReward) / (division + ratingPlus);    //    0.35 = (344 + 10) / 1001

            BossyReward += 10 + (10 * cof); // (float)bossyDef   13.5 = 10 + (10 * x)  =   3.5 = 10 * x   0.35
        }

        SendGameResults(BossyReward, RatingReward);
    }
    [Command]
    public void SendGameResults(float bossyReward, int ratingReward)
    {
        BossyReward = bossyReward;
        RatingReward = ratingReward;
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