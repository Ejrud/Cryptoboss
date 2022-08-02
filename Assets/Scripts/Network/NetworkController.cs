using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using System;

public class NetworkController : NetworkManager
{
    public enum AddressMode { Altrp, Localhost }
    public enum ServerMode { Server, Client }
    [Header("Build preferences")]
    [SerializeField] private AddressMode hostMode;
    [SerializeField] private ServerMode serverMode;

    [Header("Game controller")]
    [SerializeField] private GameProcessManagement gameProcessManagement;
    [SerializeField] private User user;

    [Header("Network objects")]
    [SerializeField] private InputField msgField;
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject exitWindow;
    private List<GameObject> _oneByOne = new List<GameObject>();
    private List<GameObject> _twoByTwo = new List<GameObject>();
    private List<GameObject> _threeByThree = new List<GameObject>();
    public List<Session> Sessions = new List<Session>();
    public List<PlayerNet> Players = new List<PlayerNet>();

    private NetworkConnection connection;
    private bool playerSpawned;
    private bool playerConnected;

    private float serverOff = 5;
    private bool isServer;
    private void Start()
    {
        exitWindow.SetActive(false);

        if (hostMode == AddressMode.Altrp)
        {
            networkAddress = "cryptoboss.win"; //     cryptoboss.win/site/public/storage/serv/  cryptoboss.win  154.12.235.119
        }
        else if (hostMode == AddressMode.Localhost)
        {
            networkAddress = "localhost";            
        }
        else
        {
            networkAddress = "cryptoboss.win";
        }

        if (serverMode == ServerMode.Client)
        {
            StartClient();
        }
        else
        {
            Debug.Log("Server started");
            StartServer();
            isServer = true;
            exitWindow.SetActive(false);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        buttons.SetActive(false);
        NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter); 
    }
    
    // Спавн игрока и инициализация режима
    public void OnCreateCharacter(NetworkConnectionToClient conn, PosMessage message)                   
    {
        GameObject playerNetObject = Instantiate(playerPrefab, message.vector2, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, playerNetObject);
        PlayerNet player = playerNetObject.GetComponent<PlayerNet>();
        player.GetGameMode();
    }

    // Когда игрок определит режим игры, про произойдет сортировка
    public void SetDistribution(PlayerNet player, bool hasEnergy)
    {
        CheckPlayers(); // Удаление пустых слотов 

        bool repeatConnect = false; 
        Session oldSession = new Session();

        if (!hasEnergy)
        {
            Destroy(player.gameObject);
            return;
        }

        foreach (PlayerNet playerNet in Players)
        {
            if (playerNet.Wallet == player.Wallet || playerNet.ChipId == player.ChipId)
            {
                Destroy(player.gameObject);
                return;
            }
        }

        if (player)
        {
            Players.Add(player);

            foreach (Session session in Sessions) // Подключение к прошлой сессии если игрок вышел
            {
                for (int i = 0; i < session.PlayerNets.Length; i++)
                {
                    if (session.StatsHolder[i].ChipId == player.ChipId)
                    {
                        session.PlayerNets[i] = player;

                        List<GameObject> players = new List<GameObject>();
                        for (int j = 0; j < session.PlayerNets.Length; j++)
                        {
                            players.Add(session.PlayerNets[j].gameObject);
                        }

                        gameProcessManagement.PrepareSession(players, session.GameMode, session);
                        oldSession = session;
                        
                        repeatConnect = true;
                        break;
                    }
                }
            }

            if (!repeatConnect)
            {
                switch (player.GameMode)
                {
                    case "one": // Режим один на один
                        
                        _oneByOne.RemoveAll(p => p.gameObject == null); // Чистка списка где элемент списка является null

                        _oneByOne.Add(player.gameObject);

                        if (_oneByOne.Count >= 2)
                        {
                            gameProcessManagement.PrepareSession(_oneByOne, "one");
                            _oneByOne = new List<GameObject>();
                        }

                        break;

                    case "two": // Режим два игрока на два игрока
                        _twoByTwo.RemoveAll(p => p.gameObject == null);

                        _twoByTwo.Add(player.gameObject);
                        Debug.Log("Game mode two = " + _twoByTwo.Count + " players");

                        if (_twoByTwo.Count >= 4)
                        {
                            gameProcessManagement.PrepareSession(_twoByTwo, "two");
                            _twoByTwo = new List<GameObject>();
                        }

                        break;

                    case "three": // Режим один на один 3 фишки против 3 фишек соперника
                        _threeByThree.RemoveAll(p => p.gameObject == null); 

                        _threeByThree.Add(player.gameObject);

                        if (_threeByThree.Count >= 2)
                        {
                            gameProcessManagement.PrepareSession(_threeByThree, "two");
                            _threeByThree = new List<GameObject>();
                        }
                        break;
                }
            }
            else
            {
                Sessions.Remove(oldSession);
            }
        }
    }

    public void ActivatePlayerSpawn()
    {
        Vector3 pos = Vector3.zero;
        PosMessage message = new PosMessage() { vector2 = pos };
        connection.Send(message);
        playerSpawned = true;
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        connection = conn;
        playerConnected = true;
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        CheckPlayers();
    }

    public void OnPlayerTwoModeDisconnect(GameObject player)
    {
        if (_twoByTwo.Contains(player))
        {
            _twoByTwo.Remove(player);
        }
    }

    private void Update()
    {
        if (!playerSpawned && playerConnected)
        {
            ActivatePlayerSpawn();
        }

        if (serverOff <= 0)
        {
            exitWindow.SetActive(true);
        }
        else
        {
            serverOff -= Time.deltaTime;
        }
    }

    public void Exit()
    {
        NetworkManager.singleton.StopClient();
        GameObject netObj = FindObjectOfType<NetworkManager>().gameObject;
        Destroy(netObj);
        SceneManager.LoadScene(0);
    }

    public void HideExitWindow()
    {
        exitWindow.SetActive(false);
    }

    public void RemovePlayer(PlayerNet player)
    {
        if(Players.Contains(player))
        {
            Players.Remove(player);
        }
    }

    public void CheckPlayers()
    {
        Players.RemoveAll(p => p == null);
    }

    private void OnApplicationQuit()
    {
        if (user)
        {
            user.Authorized = false;
        }
    }
}

public struct PosMessage : NetworkMessage
{
    public Vector2 vector2;
}