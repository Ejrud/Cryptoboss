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

    [Header("Network objects")]
    [SerializeField] private InputField msgField;
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject exitWindow;
    private GameObject[] _oneByOne = new GameObject[2];
    private List<GameObject> _twoByTwo = new List<GameObject>();
    private GameObject[] _threeByThree = new GameObject[2];
    public List<Session> Sessions = new List<Session>();
    public List<string> Wallets = new List<string>();
    

    private NetworkConnection connection;
    private bool playerSpawned;
    private bool playerConnected;
    private int connectedCountOne = 0;
    private int connectedCountThree = 0;

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
            networkAddress = "localhost";
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
    public void SetDistribution(PlayerNet player)
    {
        bool repeatConnect = false; 
        Session oldSession = new Session();

        if (Wallets.Contains(player.Wallet))
        {
            player.Understudy = true; // Если кошелек повторяется, то кошелек в списке не удалять
            Destroy(player.gameObject);
        }
        else
        {
            Wallets.Add(player.Wallet);

            foreach (Session session in Sessions)
            {
                for (int i = 0; i < session.PlayerNets.Length; i++)
                {
                    Debug.Log(session.StatsHolder[i].ChipId +  "    " + player.ChipId);

                    if (session.StatsHolder[i].ChipId == player.ChipId)
                    {
                        session.PlayerNets[i] = player;

                        GameObject[] players = new GameObject[session.PlayerNets.Length];

                        Debug.Log("Problem index = " + i);
                        
                        for (int j = 0; j < players.Length; j++)
                        {
                            players[j] = session.PlayerNets[j].gameObject;
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
                        _oneByOne[connectedCountOne] = player.gameObject;
                        connectedCountOne++;
                        Debug.Log(connectedCountOne);

                        if (connectedCountOne >= 2)
                        {
                            gameProcessManagement.PrepareSession(_oneByOne, "one");
                            connectedCountOne = 0;
                        }
                        break;

                    case "two": // Режим два игрока на два игрока
                        _twoByTwo.Add(player.gameObject);
                        Debug.Log("Game mode two = " + _twoByTwo.Count + " players");

                        if (_twoByTwo.Count >= 4)
                        {
                            GameObject[] twoByTwoPlayers = new GameObject[4];

                            for (int i = 0; i < _twoByTwo.Count; i++)
                            {
                                twoByTwoPlayers[i] = _twoByTwo[i];
                            }

                            gameProcessManagement.PrepareSession(twoByTwoPlayers, "two");
                        }
                        break;

                    case "three": // Режим один на один 3 фишки против 3 фишек соперника
                        _threeByThree[connectedCountThree] = player.gameObject;
                        connectedCountThree++;

                        if (connectedCountThree >= 2)
                        {
                            gameProcessManagement.PrepareSession(_threeByThree, "two");
                            connectedCountThree = 0;
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
        //connectedCount--;
        
        if (_oneByOne[1] == null) // 1 фишка против 1 фишки
        {
            connectedCountOne = 0; 
        }
        // if (_twoByTwo[1] != null) // 2 игрока против 2 игроков // OnPlayerTwoModeDisconnect + PlayerNet OnDestroy
        // {

        // }
        if (_threeByThree[1] == null) // 2 игрока против 2 игроков
        {
            connectedCountThree = 0; 
        }
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

    public void RemoveWallet(string wallet)
    {
        if (Wallets.Contains(wallet))
        {
            Wallets.Remove(wallet);
        }
    }
}

public struct PosMessage : NetworkMessage
{
    public Vector2 vector2;
}