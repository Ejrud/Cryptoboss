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
    public bool Ready;        
    public bool Finished;
    public bool WalletsRecieved;

    public bool Correction;
    public int PlayerIndexQueue;             
    public PlayerNet[] PlayerNets;      
    public PlayerNet[] RecoverPlayerNets;

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
    
    public void Init(PlayerNet[] players, GameProcessManagement manager, bool debugMode = false) 
    {
        // Инициализация сессии и присвоение параметров
        this.debugMode = debugMode;
        this.manager = manager;
        PlayerNets = players;

        timer = GetComponent<SessionTimer>();

        // Подготовка игроков
        StartCoroutine(WalletRecieve());

        // Первый кто выбирает карту
        PlayerIndexQueue = UnityEngine.Random.Range(0, PlayerNets.Length);
    }

    public void UpdateSession()
    {
        if (!Finished)
        {
            if (PlayerNets[0] == null || PlayerNets[1] == null && gameStarted)
            {
                FinishTheGame(false, false, true);
            }
            else if (PlayerNets[PlayerIndexQueue].CardSelected)
            {
                PlayerNets[PlayerIndexQueue].CardSelected = false;

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
            else if (PlayerNets[PlayerIndexQueue].Morale < 1) // 
            {
                PlayerNets[PlayerIndexQueue].Morale += energyRecovery;
                SetNextIndexQueue();
                
                PlayerNets[0].UpdatePlayerCharacteristic(PlayerNets[0].Capital, PlayerNets[0].Morale, PlayerNets[1].Capital, PlayerNets[1].Morale, PlayerNets[0].MaxEnergy);
                PlayerNets[1].UpdatePlayerCharacteristic(PlayerNets[1].Capital, PlayerNets[1].Morale, PlayerNets[0].Capital, PlayerNets[0].Morale, PlayerNets[1].MaxEnergy);
            }
        }
        else
        {
            if (PlayerNets[0] == null && PlayerNets[1] == null)
            {
                Destroy(gameObject);
            }
        }
    }

    public void PrepareNextRound(bool correction = false)
    {
        SetNextIndexQueue(correction);
        
        for (int i = 0; i < PlayerNets.Length; i++)
        {
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
            timer.isStoped = true;
            PlayerNets[0].StopGame("Other player disconnected", "", 0f, "0", true);
            PlayerNets[1].StopGame("Other player disconnected", "", 0f, "0", true);
        }
    }

    private IEnumerator SetReward(string winnerWallet, string winnerGuid, string looseGuid, string mode)
    {
        WWWForm form = new WWWForm();
        
        form.AddField("WinGuid", winnerGuid);
        form.AddField("WinWallet", winnerWallet);
        form.AddField("LooseGuid", looseGuid);
        form.AddField("Mode", mode);

        // �������� ����
        using (UnityWebRequest www = UnityWebRequest.Post(seUrl + "accrual.php", form))
        { 
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                DataBaseResult results = JsonConvert.DeserializeObject<DataBaseResult>(www.downloadHandler.text);

                if (PlayerNets[0].Win && !PlayerNets[1].Win)
                {
                    PlayerNets[0].StopGame("YOU HAVE WON!", "+", results.bossy, results.rating, false);
                    PlayerNets[1].StopGame("YOU LOSE", "-", 0f, results.decrement, false);
                }
                else if (!PlayerNets[0].Win && PlayerNets[1].Win)
                {
                    PlayerNets[1].StopGame("YOU HAVE WON!", "+", results.bossy, results.rating, false);
                    PlayerNets[0].StopGame("YOU LOSE", "-", 0f ,results.decrement, false);
                }
                else if (!PlayerNets[0].Win && !PlayerNets[1].Win)
                {
                    PlayerNets[0].StopGame("DRAW", "", 0f, "0", false);
                    PlayerNets[1].StopGame("DRAW", "", 0f, "0", false);
                }
                else
                {
                    PlayerNets[0].StopGame("DRAW", "", 0f, "0", false);
                    PlayerNets[1].StopGame("DRAW", "", 0f, "0", false);
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
        CardData cards = PlayerNets[playerIndex].HandCards[PlayerNets[playerIndex].SelectedCardId];
        return cards;
    }

    private void SetNextIndexQueue(bool correction = false)
    {
        if (!correction)
        {
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
    ////////////////////////////////// Вызвается единожды ////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region Prepairing players (called once)
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

            if (walletRecieved == PlayerNets.Length)
            {
                StartCoroutine(Preparation());
                WalletsRecieved = true;
            }

            yield return new WaitForSeconds(1f);
        }

        yield return null;
    }

    private IEnumerator Preparation() 
    {
        // Если игроков более 2 на сессию, то помимо загрузки изображений, определить друзей. Последний индекс массива - друг (0, 2 - игроки 1  /vs/  1, 3 - игроки 2)
        if (PlayerNets.Length > 2)
        {
            PlayerNets[0].Friend = PlayerNets[2];
            PlayerNets[2].Friend = PlayerNets[0];
            PlayerNets[1].Friend = PlayerNets[3];
            PlayerNets[3].Friend = PlayerNets[1];

            int[] pId_1 = { PlayerNets[1].ChipId, PlayerNets[3].ChipId, PlayerNets[2].ChipId }; // 0
            int[] pId_2 = { PlayerNets[0].ChipId, PlayerNets[2].ChipId, PlayerNets[3].ChipId }; // 1
            int[] pId_3 = { PlayerNets[1].ChipId, PlayerNets[3].ChipId, PlayerNets[0].ChipId }; // 2
            int[] pId_4 = { PlayerNets[0].ChipId, PlayerNets[2].ChipId, PlayerNets[1].ChipId }; // 3

            PlayerNets[0].LoadRivalChip(pId_1, "two");
            PlayerNets[1].LoadRivalChip(pId_2, "two");
            PlayerNets[2].LoadRivalChip(pId_3, "two");
            PlayerNets[3].LoadRivalChip(pId_4, "two");
        }
        else
        {
            int[] pId_1 = { PlayerNets[1].ChipId };
            int[] pId_2 = { PlayerNets[0].ChipId };

            PlayerNets[0].LoadRivalChip(pId_1, "one");
            PlayerNets[1].LoadRivalChip(pId_2, "one");
        }

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
        
        yield return new WaitForSeconds(5f);
        
        for (int i = 0; i < PlayerNets.Length; i++)
        {
            PlayerNets[i].RepresentationWindow(false);
        }
        
        timer.StartTimer();
        yield return null;
    }

    private IEnumerator PreparePlayers()
    {
        // Цикл загрузки данных с бд
        for (int i = 0; i < PlayerNets.Length; i++)
        {
            WWWForm form = new WWWForm();
            form.AddField("guid", "CryptoBoss #" + PlayerNets[i].ChipId); // 

            using (UnityWebRequest www = UnityWebRequest.Post(seUrl + "get_cards.php", form)) // Загрузка карт
            { 
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    Debug.Log(json);
                    List<Cards> dbCard = JsonConvert.DeserializeObject<List<Cards>>(json);
                    
                    PlayerNets[i].CardCollection = GetCardDeck(dbCard);
                }
                else
                { 
                    Debug.Log("Incorrect data");
                    Debug.Log(www.error);
                }
            }

            using (UnityWebRequest www = UnityWebRequest.Post(seUrl + "get_health.php", form)) // Загрузка здоровья
            { 
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    Debug.Log(json);
                    List<UserHealth> health = JsonConvert.DeserializeObject<List<UserHealth>>(json);

                    PlayerNets[i].MaxHealth = Convert.ToInt32(health[0].capital_max);

                    Debug.Log($"Player {i + 1} health: " + PlayerNets[i].MaxHealth);
                }
                else
                { 
                    Debug.Log("Incorrect data");
                    Debug.Log(www.error);
                }
            }
                        
            yield return null; 
        }

        WWWForm emptyForm = new WWWForm(); // Загрузка длительности сессии

        using (UnityWebRequest www = UnityWebRequest.Post(seUrl + "game_params.php", emptyForm)) // "a0664627.xsph.ru/cryptoboss_back/"
        { 
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                
                List<GameParams> gameParams = JsonConvert.DeserializeObject<List<GameParams>>(json);
                timer.RoundTimer = Convert.ToInt32(gameParams[0].timer);
                timer.OriginalTime = timer.RoundTimer;
                Debug.Log("Session timer = " + timer.RoundTimer);
            }
            else
            { 
                timer.RoundTimer = 10;
                Debug.Log("Incorrect data");
                Debug.Log(www.error);
            }
        }

        if(PlayerNets.Length > 2) // Режим 2 на 2
        {
            for (int i = 0; i < PlayerNets.Length / 2; i++)
            {
                PlayerNets[i].MaxHealth = PlayerNets[i].MaxHealth + PlayerNets[i].Friend.MaxHealth / 2; // Присвоение нового максимального здоровья
                PlayerNets[i].Capital = PlayerNets[i].MaxHealth;                                        // Присвоение максимального капитала к текущему капиталу
                PlayerNets[i].Friend.MaxHealth = PlayerNets[i].MaxHealth;                               // Присвоение максимального капитала союзнику
                PlayerNets[i].Friend.Capital = PlayerNets[i].Friend.MaxHealth;                          // Присвоение союзнику максимального капитала к текущему капиталу
            }

            PlayerNets[0].UpdatePlayerCharacteristic(PlayerNets[0].Capital, 20, PlayerNets[1].Capital, 20, 20);     // Правило 3-х "п" похер, потом переделаем
            PlayerNets[1].UpdatePlayerCharacteristic(PlayerNets[1].Capital, 20, PlayerNets[0].Capital, 20, 20);     // Синхронизированные капиталы игроков с инденксами:
            PlayerNets[2].UpdatePlayerCharacteristic(PlayerNets[2].Capital, 20, PlayerNets[1].Capital, 20, 20);     // (0, 2)      (1, 3)
            PlayerNets[3].UpdatePlayerCharacteristic(PlayerNets[3].Capital, 20, PlayerNets[0].Capital, 20, 20);
            
        }
        else // Режим 1 на 1 ( + возможно 3 на 3)
        {
            PlayerNets[0].Capital = PlayerNets[0].MaxHealth;   
            PlayerNets[1].Capital = PlayerNets[1].MaxHealth;   

            PlayerNets[0].UpdatePlayerCharacteristic(PlayerNets[0].Capital, 20, PlayerNets[1].Capital, 20, 20);
            PlayerNets[1].UpdatePlayerCharacteristic(PlayerNets[1].Capital, 20, PlayerNets[0].Capital, 20, 20);
        }

        PrepareNextRound();

        gameStarted = true;
        // Ready = true;
        yield return null; 
    }

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

    public class GameParams
    {
        public string timer { get; set; }
    }

    #endregion
}
