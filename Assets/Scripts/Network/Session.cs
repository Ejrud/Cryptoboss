using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mirror;
using System;
using System.Security.Permissions;
using Newtonsoft.Json;

public class Session : MonoBehaviour
{
    // Если пользователи отправили номера своих кошельков на сервер, то сессия готова
    public bool Ready;        
    // Если сессия полностью завершена, то GameProcessManagment перестанет к нему обращаться          
    public bool Finished;
    // Если все кошельки собраны, то в GameProcessManagment происходит подбор карт (отправляется запрос к серверу)              
    public bool WalletsRecieved;

    public bool Correction;
    // Какой игрок идет    
    public int PlayerIndexQueue;             
    // Сетевой объект игрока (Хранит в себе характеристики игрока)
    public PlayerNet[] PlayerNets;      

    private GameProcessManagement manager;
    private SessionTimer timer;

    [Header("TypeStage")]
    private bool debugMode;

    [SerializeField] private ChipData debugData;
    private bool gameStarted = false;
    private bool chipIdRecieved;

    private string seUrl = "https://cryptoboss.win/game/back/"; // a0664627.xsph.ru/cryptoboss_back/     //   https://cryptoboss.win/game/back/

    [Header("Settings")]
    [SerializeField] private float energyRecovery = 0.5f;
    
    // Инициализирование характеристик игрока
    public void Init(PlayerNet[] players, GameProcessManagement manager, bool debugMode = false) 
    {
        this.debugMode = debugMode;
        this.manager = manager;
        PlayerNets = players;

        timer = GetComponent<SessionTimer>();

        StartCoroutine(WalletRecieve());
        PlayerIndexQueue = UnityEngine.Random.Range(0, PlayerNets.Length);
    }

    // Обновление сессии. Проверка на готовность игроков к концу раунда
    public void UpdateSession()
    {
        // Если сессия не закончена, то сессия не удаляется
        if (!Finished)
        {
            // Если в процессе игры вышел игрок, то сессия заканчивается
            if (PlayerNets[0] == null || PlayerNets[1] == null && gameStarted)
            {
                FinishTheGame(false, false, true);
            }
            

            // Если игрок выбрал карту для игры, то сесия готова к вычислениям
            else if (PlayerNets[PlayerIndexQueue].CardSelected)
            {
                PlayerNets[PlayerIndexQueue].CardSelected = false;

                // Копии выбранных карт игроками
                CardData selectedCard = GetSelectedCards(PlayerIndexQueue);

                for (int i = 0; i < PlayerNets.Length; i++)
                {
                    if (i != PlayerIndexQueue)
                    {
                        PlayerNets[i].RecieveRivalCards(PlayerNets[PlayerIndexQueue].SelectedCardId, selectedCard);
                    }
                }

                timer.isStoped = true;
                Ready = true;
            }
            else if (PlayerNets[PlayerIndexQueue].Morale < 1) // Проверка на количество энергии
            {
                PlayerNets[PlayerIndexQueue].Morale += energyRecovery;
                SetNextIndexQueue();
                
                PlayerNets[0].UpdatePlayerCharacteristic(PlayerNets[0].Capital, PlayerNets[0].Morale, PlayerNets[1].Capital, PlayerNets[1].Morale, PlayerNets[0].MaxEnergy);
                PlayerNets[1].UpdatePlayerCharacteristic(PlayerNets[1].Capital, PlayerNets[1].Morale, PlayerNets[0].Capital, PlayerNets[0].Morale, PlayerNets[1].MaxEnergy);
            }
        }
        // Если сессия закончена, то она удаляется
        else
        {
            if (PlayerNets[0] == null && PlayerNets[1] == null)
            {
                Destroy(gameObject);
            }
        }
    }

    // Генерация 4 рандомных карт из колоды игрока (Вызывается в BattleManager)
    public void PrepareNextRound(bool correction = false) // correction тип крты позволяющий сделать повторный ход
    {
        SetNextIndexQueue(correction);
        
        // Проверка на кол-во карт у игроков (если меньше или равно 2, то добавляются еще 3 карты)
        for (int i = 0; i < PlayerNets.Length; i++)
        {
            // Проверка на пустые слоты в руке
            if (PlayerNets[i].UsedCount > 2)
            {
                continue;
            }

            PlayerNets[i].CardCollection = Shuffle(PlayerNets[i].CardCollection);
            List<CardData> Cards = new List<CardData>(); 

            if (PlayerNets[i].FirstStart)
            {
                for (int j = 0; j < 5; j++)
                {
                    PlayerNets[i].HandCards[j] = PlayerNets[i].CardCollection[j];
                    PlayerNets[i].HandCards[j].Used = false;
                }
            }
            else
            {
                for (int j = 0; j < PlayerNets[i].CardCollection.Length; j++)
                {
                    bool repeat = false;
                    for (int k = 0; k < PlayerNets[i].HandCards.Length; k++)
                    {
                        if (PlayerNets[i].CardCollection[j].Guid == PlayerNets[i].HandCards[k].Guid)
                        {
                            repeat = true;
                        }
                    }
                    if (!repeat)
                    {
                        Cards.Add(PlayerNets[i].CardCollection[j]);
                    }
                }

                for (int j = 0; j < 5; j++)
                {
                    if (!PlayerNets[i].HandCards[j].Used) continue;
                    
                    PlayerNets[i].HandCards[j] = Cards[j];
                    PlayerNets[i].HandCards[j].Used = false;
                }
            }

            Debug.Log("Player prepared");

            PlayerNets[i].FirstStart = false;
            PlayerNets[i].UpdateRoundCards(PlayerNets[i].HandCards);
            PlayerNets[i].CloseWindowWaitingForPlayers();
        }

        Ready = false;
        timer.ResetTimer();
    }

    public void UpdatePlayerTimer(string time)
    {
        foreach (PlayerNet player in PlayerNets)
        {
            player.SetTimerText(time);
        }
    }
    
    public void FinishTheGame(bool playerWin_1, bool playerWin_2, bool playerDisconnected = false)
    {
        Ready = false;
        Finished = true;

        if (!playerDisconnected)
        {
            // Кто выиграл, а кто проиграл
            PlayerNets[0].Win = playerWin_1;
            PlayerNets[1].Win = playerWin_2;

            string winnerWallet = "";
            string winnerGuid = "";
            string looseGuid = ""; 
            string mode = "one";
        
            if (PlayerNets[0].Win)
            {
                winnerWallet = PlayerNets[0].Wallet;
                winnerGuid = "CryptoBoss #" + PlayerNets[0].ChipId;
                looseGuid = "CryptoBoss #" + PlayerNets[1].ChipId;

            }
            else if (PlayerNets[1].Win)
            {
                winnerWallet = PlayerNets[1].Wallet;
                winnerGuid = "CryptoBoss #" + PlayerNets[1].ChipId;
                looseGuid = "CryptoBoss #" + PlayerNets[0].ChipId;
            }

            StartCoroutine(SetReward(winnerWallet, winnerGuid, looseGuid, mode));
        }
        else
        {
            PlayerNets[0].EndGame("Other player disconnected", "", 0f, "0");
            PlayerNets[1].EndGame("Other player disconnected", "", 0f, "0");
        }
    }

    private IEnumerator SetReward(string winnerWallet, string winnerGuid, string looseGuid, string mode)
    {
        WWWForm form = new WWWForm();
        
        form.AddField("WinGuid", winnerGuid);
        form.AddField("WinWallet", winnerWallet);
        form.AddField("LooseGuid", looseGuid);
        form.AddField("Mode", mode);

        // Загрузка карт
        using (UnityWebRequest www = UnityWebRequest.Post(seUrl + "accrual.php", form))
        { 
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                DataBaseResult results = JsonConvert.DeserializeObject<DataBaseResult>(www.downloadHandler.text);

                if (PlayerNets[0].Win && !PlayerNets[1].Win)
                {
                    PlayerNets[0].EndGame("YOU HAVE WON!", "+", results.bossy, results.rating);
                    PlayerNets[1].EndGame("YOU LOOSE", "-", 0f, results.decrement);
                }
                else if (!PlayerNets[0].Win && PlayerNets[1].Win)
                {
                    PlayerNets[1].EndGame("YOU HAVE WON!", "+", results.bossy, results.rating);
                    PlayerNets[0].EndGame("YOU LOOSE", "-", 0f ,results.decrement);
                }
                else if (!PlayerNets[0].Win && !PlayerNets[1].Win)
                {
                    PlayerNets[0].EndGame("DRAW", "", 0f, "0");
                    PlayerNets[1].EndGame("DRAW", "", 0f, "0");
                }
                else
                {
                    PlayerNets[0].EndGame("DRAW", "", 0f, "0");
                    PlayerNets[1].EndGame("DRAW", "", 0f, "0");
                }

                Debug.Log("Reward = " + results.bossy);
            }
            else
            { 
                Debug.Log("Incorrect data");
                Debug.Log(www.error);
            }
        }
    }

    public void EndRound()
    {   
        PrepareNextRound();
    }
    
    private CardData GetSelectedCards(int playerIndex)
    {
        // Подбор характеристик карт по id'шникам (RoundCards в своих индексах содежит индексы карт)
        CardData cards = PlayerNets[playerIndex].HandCards[PlayerNets[playerIndex].SelectedCardId];
        return cards;
    }

    private void SetNextIndexQueue(bool correction = false)
    {
        if (!correction)
        {
            // Выбирается след. игрок
            PlayerIndexQueue++;
            if (PlayerIndexQueue >= PlayerNets.Length)
            {
                PlayerIndexQueue = 0;
            }
        }

        for(int i = 0; i < PlayerNets.Length; i++)
        {
            if (i == PlayerIndexQueue)
            {
                PlayerNets[i].MyTurn = true;
            }
            else
            {
                PlayerNets[i].MyTurn = false;
            }

            PlayerNets[i].SetTurn(PlayerNets[i].MyTurn);
        }
    }
    
    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Подготовка игроков ////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region Prepairing players (called once)
    // Подбор и сохранения номеров кошельков игроков (Ниже идет основная логика подготовки игроков к сессии)
    private IEnumerator WalletRecieve()
    {
        while (!WalletsRecieved)
        {
            int walletRecieved = 0;

            for (int i = 0; i < PlayerNets.Length; i++)
            {
                if (!string.IsNullOrEmpty(PlayerNets[i].Wallet))
                {
                    walletRecieved++;
                }
            }

            // Если были найдены все кошельки пользователей, то они отправляются на сервер
            if (walletRecieved == PlayerNets.Length)
            {
                StartCoroutine(GetPlayerCards());
                WalletsRecieved = true;
            }

            yield return new WaitForSeconds(1f);
        }

        yield return null;
    }

    // Загрузка карт из бд (Debug генерирует рандомные карты)(Так же инициализирует параметры игроков)
    private IEnumerator GetPlayerCards()
    {
        // Для других режимов добавить условие на кол-во игроков
        // Загрузка изображений для фишек
        PlayerNets[0].LoadRivalChip(PlayerNets[1].ChipId);
        PlayerNets[1].LoadRivalChip(PlayerNets[0].ChipId);

        while (!chipIdRecieved)
        {
            int chipRecieved = 0;

            for (int i = 0; i < PlayerNets.Length; i++)
            {
                if (PlayerNets[i].ChipReceived)
                {
                    chipRecieved++;
                }
            }

            if (chipRecieved >= PlayerNets.Length)
            {
                chipIdRecieved = true;
            }

            yield return new WaitForUpdate();
        }

        for (int i = 0; i < PlayerNets.Length; i++)
        {
            PlayerNets[i].CloseWindowWaitingForPlayers();
            PlayerNets[i].RepresentationWindow(true);
        }

        StartCoroutine(PreparePlayers());
        
        // Показ фишек игроков перед игрой
        yield return new WaitForSeconds(5f);

        PlayerNets[0].RepresentationWindow(false);
        PlayerNets[1].RepresentationWindow(false);
        
        timer.StartTimer();
        yield return null;
    }

    // Вызывается единожды. Предподготовка игроков к первому раунду (присваивание здоровья, энергии и прочего)
    private IEnumerator PreparePlayers()
    {
        int[] playerHealth = new int[PlayerNets.Length];

        for (int i = 0; i < PlayerNets.Length; i++)
        {
            // Отправка запроса на сервер
            WWWForm form = new WWWForm();
            form.AddField("guid", "CryptoBoss #" + PlayerNets[i].ChipId); // 

            // Загрузка карт
            using (UnityWebRequest www = UnityWebRequest.Post(seUrl + "get_cards.php", form))
            { 
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    CardData[] cards = new CardData[10];

                    string json = www.downloadHandler.text;
                    Debug.Log(json);
                    List<Cards> dbCard = JsonConvert.DeserializeObject<List<Cards>>(json);

                    if (!debugMode)
                    {
                        PlayerNets[i].CardCollection = GetCardDeck(dbCard);
                    }
                    else
                    {
                        PlayerNets[i].CardCollection = debugData.CardDeck;
                    }
                }
                else
                { 
                    Debug.Log("Incorrect data");
                    Debug.Log(www.error);
                }
            }

            // Загрузка здоровья
            using (UnityWebRequest www = UnityWebRequest.Post(seUrl + "get_health.php", form))
            { 
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    Debug.Log(json);
                    List<UserHealth> health = JsonConvert.DeserializeObject<List<UserHealth>>(json);

                    playerHealth[i] = Convert.ToInt32(health[0].capital_max);
                    PlayerNets[i].MaxHealth = playerHealth[i];

                    Debug.Log($"Player {i + 1} health: " + playerHealth[i]);
                }
                else
                { 
                    Debug.Log("Incorrect data");
                    Debug.Log(www.error);
                }
            }
                        
            yield return null; 
        }

        // здоровье, выносливость, здоровье соперника
        PlayerNets[0].UpdatePlayerCharacteristic(playerHealth[0], 20, playerHealth[1], 20, 20);
        PlayerNets[1].UpdatePlayerCharacteristic(playerHealth[1], 20, playerHealth[0], 20, 20);

        PrepareNextRound();

        gameStarted = true;
        // Ready = true;
        yield return null; 
    }

    // Выдача карт под конкретную фишку
    private CardData[] GetCardDeck(List<Cards> dbCards)
    {
        CardData[] cards = new CardData[dbCards.Count];

        for (int i = 0; i < cards.Length; i++)
        {
            CardData card = new CardData();

            card.Name = dbCards[i].name;
            card.Type = dbCards[i].type;
            card.EnergyDamage = Convert.ToInt32(dbCards[i].loss_energy);
            card.CapitalDamage = Convert.ToInt32(dbCards[i].loss_capital);
            card.EnergyHealth = Convert.ToInt32(dbCards[i].heal_energy);
            card.CapitalEarnings = Convert.ToInt32(dbCards[i].profit);
            card.DamageResistance = Convert.ToInt32(dbCards[i].armor_of_loss);
            card.CardCost = Convert.ToInt32(dbCards[i].energy_cost);
            card.Guid = dbCards[i].card_id;

            cards[i] = card;
        }

        return Shuffle(cards);
    }

    private CardData[] Shuffle(CardData[] arr)
    {
        System.Random rand = new System.Random();
    
        for (int i = arr.Length - 1; i >= 1; i--)
        {
            int j = rand.Next(i + 1);
    
            CardData tmp = arr[j];
            arr[j] = arr[i];
            arr[i] = tmp;
        }

        return arr;
    }

    private void OnDestroy()
    {
        manager.RemoveSession(this);
    }

    public class Cards
    {
        public string type { get; set; }
        public string name { get; set; }
        public string loss_energy { get; set; }
        public string loss_capital { get; set; }
        public string heal_energy { get; set; }
        public string profit { get; set; }
        public string armor_of_loss { get; set; }
        public string energy_cost { get; set; }
        public string card_id { get; set; }
    }

    public class UserHealth
    {
        public string capital_max { get; set; }
    }
    public class DataBaseResult
    {
        public string rating { get; set; }
        public string decrement { get; set; }
        public float bossy { get; set; }
    }

    #endregion
}
