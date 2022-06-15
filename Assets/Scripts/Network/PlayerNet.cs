using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Mirror;
using System;
public class PlayerNet : NetworkBehaviour
{
    [Header("Player parameters")]
    public string Wallet;
    public int ChipId;
    public CardData[] CardCollection;     // Все карты пользователя (или карты фишки)
    public CardData[] HandCards;         // Карты в руке
    public CardData[] PreviousCards;
    public CardData PreviousCard;
    public CardData RivalCard;
    public int HedgeFundCount = 0;

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
    public int SelectedCardId;       // Вызывается на сервере
    public int UsedCount;
    
    private string seUrl = "https://cryptoboss.win/game/back/"; // http://a0664627.xsph.ru/cryptoboss_back/images/  // https://cryptoboss.win/game/back/images/

    #region UI elements
    [Header("player UI")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text energyText;
    [SerializeField] private Text exitWindowText;
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

    // Отключение видимости игрока от всех и отправка на сервер свой идентификатор
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
            
            CmdSendWalletAndId(Wallet, ChipId, ChipReceived);

            Texture chipTexture = lastTexture;

            // Подгрузка фишки пользователя (из локальных данных этого пользователя)
            for (int i = 0; i < user.chipDatas.Length; i++)
            {

                if (ChipId == user.chipDatas[i].Id)
                {
                    chipTexture = user.chipDatas[i].ChipTexture;

                    foreach(RawImage chipImage in userChipImage) // Обновление всех фишек на экране
                    {
                        chipImage.texture = chipTexture;
                    }
                    Debug.Log("Chip loaded = " + ChipId);
                    return;
                }
                else    // При первом запуске с ParrelSync id фишки не задано пожтому задается рандомная
                {
                    Debug.Log("ChipId not found. prefab id = " +  ChipId);

                    chipTexture = user.chipDatas[UnityEngine.Random.Range(0, user.chipDatas.Length - 1)].ChipTexture;

                    foreach(RawImage chipImage in userChipImage) // Обновление всех фишек на экране
                    {
                        chipImage.texture = chipTexture;
                    }
                }
            }
        }
    }

    // Показатели обновляются при первом запуске и при завершении вычисления карт игроков
    public void UpdatePlayerCharacteristic(int Capital, float Morale, int EnemyCapitale, float EnemyEnergy, float maxEnergy)
    {
        this.Capital = Capital;
        this.Morale = Morale;
        this.EnemyCapital = EnemyCapitale;
        this.EnemyEnergy = EnemyEnergy;
        this.OriginMorale = Morale;
        this.MaxEnergy = maxEnergy;

        UpdateClientParameters(this.Capital, this.Morale, this.EnemyCapital, this.EnemyEnergy, this.MaxEnergy);
    }

    // Обновляет карты в руке (Переносить остальные карты в изначчальные положения)
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

    // Обновляет интерфейс игрока
    public void UpdateUI()
    {
        float floatCapital = Convert.ToSingle(Capital);
        healthImage.fillAmount = floatCapital / 1000;
        float floatMorale = Convert.ToSingle(Morale);
        energyImage.fillAmount = floatMorale / MaxEnergy;

        // Debug.Log("Capital = " + Capital + " healthImage.fillAmount = " + Capital * 0.1f);
        healthText.text = Capital.ToString();
        energyText.text = Morale.ToString();

        float floatCapEnemy = Convert.ToSingle(EnemyCapital);
        enemyHealthImage.fillAmount = floatCapEnemy / 1000;
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

    [Command] // На сервер отправляется кошелек конкретного пользователя
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
        if (hasAuthority) // без этого на серверном экземпляре и не только будут ошибки
        {
            // В анимации прятать все карты и выпускать колличество выбранныых карт соперником]
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
    public void UpdateClientParameters(int Capital, float Morale, int EnemyCapital, float EnemyEnergy, float maxEnergy)
    {
        this.Morale = Morale;
        this.Capital = Capital;
        this.EnemyCapital = EnemyCapital;
        this.EnemyEnergy = EnemyEnergy;
        this.OriginMorale = Morale;
        this.MaxEnergy = maxEnergy;

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
        if(hasAuthority) // Имитировать кнопку send (OnSendCards подбирает оставшиеся карты)
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

            if(textureRequest.error != null) // Если не удастся загрузить текстуру, то поизойдет повторная попытка
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

    public class Response 
    {
        public string image;
    }
}