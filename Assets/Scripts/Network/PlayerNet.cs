using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Mirror;
using System;
using TMPro;

public class PlayerNet : NetworkBehaviour
{
    [Header("Player parameters")]
    public string Wallet;
    public string GameMode;
    public int ChipId;
    public CardData[] CardCollection;     // ��� ����� ������������ (��� ����� �����)
    public CardData[] HandCards;         // ����� � ����
    public CardData[] PreviousCards;
    public CardData PreviousCard;
    public CardData RivalCard;
    public int HedgeFundCount = 0;

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
    public bool AnimationComplete;
    public int SelectedCardId;       // ���������� �� �������
    public int UsedCount;
    
    private string seUrl = "https://cryptoboss.win/game/back/"; // http://a0664627.xsph.ru/cryptoboss_back/images/  // https://cryptoboss.win/game/back/images/

    #region UI elements
    [Header("player UI")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text energyText;
    [SerializeField] private TMP_Text exitWindowText;
    [SerializeField] private Text bossyText;
    [SerializeField] private Text raitingText;
    [SerializeField] private Image healthImage;
    [SerializeField] private Image energyImage;
    [SerializeField] private RawImage ChipImage;
    [SerializeField] private RawImage RivalChipImage;


    [SerializeField] private GameObject playersWaitingObj;
    [SerializeField] private GameObject representationScreen;
    [SerializeField] private GameObject backToLobbyWindow;

    [Header("Enemy UI")]
    [SerializeField] private Image enemyHealthImage;
    [SerializeField] private Image enemyEnergyImage;
    [SerializeField] private Text enemyhealthText;
    [SerializeField] private Text enemyEnergyText;

    [Header("Gameplay")]
    [SerializeField] private CardManager cardManager;
    [SerializeField] private int menuSceneIndex = 0;
    [SerializeField] private User user;
    [SerializeField] private RawImage[] userChipImage;
    [SerializeField] private RawImage[] rivalChipImages;
    [SerializeField] private Texture lastTexture;
    [SerializeField] private AudioSource audioSource;
    private Texture2D rivalChipTexture;

    #endregion

    // ���������� ��������� ������ �� ���� � �������� �� ������ ���� �������������
    private void Start()
    {
        audioSource.volume = PlayerPrefs.GetFloat("musicVolume");

        playersWaitingObj.SetActive(true);
        representationScreen.SetActive(false);

        GameObject netObject = FindObjectOfType<NetworkManager>().gameObject;
        
        gameObject.SetActive(false);

        FirstStart = true;

        if (hasAuthority)
        {
            gameObject.SetActive(true);

            Wallet = PlayerPrefs.GetString("Wallet");
            ChipId = PlayerPrefs.GetInt("chipId");
            GameMode = PlayerPrefs.GetString("GameMode");
            
            CmdSendWalletAndId(Wallet, ChipId, ChipReceived);

            Texture chipTexture = lastTexture;

            // ��������� ����� ������������ (�� ��������� ������ ����� ������������)
            for (int i = 0; i < user.ChipParam.Count; i++)
            {

                if (ChipId == user.ChipParam[i].Id)
                {
                    chipTexture = user.ChipParam[i].ChipTexture;

                    foreach(RawImage chipImage in userChipImage) // ���������� ���� ����� �� ������
                    {
                        chipImage.texture = chipTexture;
                    }
                    Debug.Log("Chip loaded = " + ChipId);
                    return;
                }
                else    // ��� ������ ������� � ParrelSync id ����� �� ������ ������� �������� ���������
                {
                    Debug.Log("ChipId not found. prefab id = " +  ChipId);

                    chipTexture = user.ChipParam[UnityEngine.Random.Range(0, user.ChipParam.Count - 1)].ChipTexture;

                    foreach(RawImage chipImage in userChipImage) // ���������� ���� ����� �� ������
                    {
                        chipImage.texture = chipTexture;
                    }
                }
            }
        }
    }

    // ���������� ����������� ��� ������ ������� � ��� ���������� ���������� ���� �������
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

    // ��������� ����� � ���� (���������� ��������� ����� � ������������ ���������)
    public void UpdateRoundCards(CardData[] hand)
    {
        HandCards = hand;
        CardSelected = false;
        UsedCount = hand.Length;
        foreach (CardData card in HandCards)
        {
            card.Used = false;
        }
        SyncRoundCards(HandCards);
    }

    // ��������� ��������� ������
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
        Destroy(netObj);
        SceneManager.LoadScene(menuSceneIndex);
    }
    [Command]
    public void CmdSendGameMode(string gameMode)
    {
        GameMode = gameMode;
        Debug.Log(GameMode);
        FindObjectOfType<NetworkController>().SetDistribution(this); // cringe
    }

    [Command] // �� ������ ������������ ������� ����������� ������������
    public void CmdSendWalletAndId(string wallet, int chipId, bool received)
    {
        Wallet = wallet;
        ChipId = chipId;
        ChipReceived = received;
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

    [ClientRpc]
    public void SetAudit(CardData[] handCards)
    {
        if (hasAuthority)
        {
            // �������� ����� ���������
            cardManager.AuditRivalCards(handCards);
        }
    }

    public void ResetAudit()
    {
        if (hasAuthority)
        {
            // ������ ����� ������
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

            if (MyTurn)
            {
                ChipImage.color = Color.white;
                RivalChipImage.color = Color.gray;
            }
            else
            {
                ChipImage.color = Color.gray;
                RivalChipImage.color = Color.white;
            }
        }
    }

    [ClientRpc]
    public void RecieveRivalCards(int cardId, CardData rivalCard)
    {
        if (hasAuthority) // ��� ����� �� ��������� ���������� � �� ������ ����� ������
        {
            // � �������� ������� ��� ����� � ��������� ����������� ���������� ���� ����������]
            RivalCard = rivalCard;
            cardManager.RivalCardSelect(cardId, RivalCard);
        }
    }

    [ClientRpc]
    public void SyncRoundCards(CardData[] roundCards)
    {
        if (hasAuthority)
        {
            Debug.Log("Local player prepared");

            HandCards = roundCards;
            CardSelected = false;
            cardManager.ReturnCards();
        }
    }

    [ClientRpc]
    public void SetTimerText(string time)
    {
        timerText.text = time;
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

        Debug.Log("Player characteristics: capital = " + this.Capital + ", morale = " + this.Morale);

        UpdateUI();
    }

    [ClientRpc]
    public void EndGame(string text, string charIncrement, float bossy, string raiting)
    {
        backToLobbyWindow.SetActive(true);
        exitWindowText.text = text;
        bossyText.text = charIncrement + bossy;
        raitingText.text = charIncrement + raiting;
    }

    [ClientRpc]
    public void TimerEnded()
    {
        if(hasAuthority) // ����������� ������ send (OnSendCards ��������� ���������� �����)
        {
            cardManager.SelectRandomCard();
        }
    }

    [ClientRpc]
    public async void LoadRivalChip(int rivalChipId) // 
    {
        if(hasAuthority)
        {
            Debug.Log("Load textures...");

            string newImgUri = seUrl + "images/" + rivalChipId + ".png";

            // fetch image and display in game
            UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(newImgUri); // imageUri
            await textureRequest.SendWebRequest();
            rivalChipTexture = ((DownloadHandlerTexture)textureRequest.downloadHandler).texture;

            if(textureRequest.error != null) // ���� �� ������� ��������� ��������, �� ��������� ��������� �������
            {
                LoadRivalChip(rivalChipId);
                return;
            }
            Debug.Log("Load complete");

            foreach (RawImage rivalImg in rivalChipImages)
            {
                rivalImg.texture = rivalChipTexture;
            }

            ChipReceived = true;
            CmdSendWalletAndId(Wallet, ChipId, ChipReceived);
        }
    }

    [ClientRpc]
    public void GetGameMode()
    {
        string gameMode = PlayerPrefs.GetString("GameMode");
        gameMode = "three";
        GameMode = gameMode;
        CmdSendGameMode(gameMode);
    }

    private void OnDestroy()
    {
        if (isServer)
        {
            if (GameMode == "two")
            {
                FindObjectOfType<NetworkController>().OnPlayerTwoModeDisconnect(this.gameObject);
            }
        }
    }

    public class Response 
    {
        public string image;
    }
    
    // Impact �������� �� ���������� �������� ������ � ��� �������
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