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
    private GameObject[] players = new GameObject[2];

    private NetworkConnection connection;
    private bool playerSpawned;
    private bool playerConnected;
    private int connectedCount = 0;

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
    
    public void OnCreateCharacter(NetworkConnectionToClient conn, PosMessage message)                   
    {
        GameObject playerNetObject = Instantiate(playerPrefab, message.vector2, Quaternion.identity);

        NetworkServer.AddPlayerForConnection(conn, playerNetObject);
        players[connectedCount] = playerNetObject;
        connectedCount++;

        if (numPlayers % 2 == 0)
        {
            gameProcessManagement.PrepareSession(players);
            connectedCount = 0;
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
        
        if (players[1] == null)
        {
            connectedCount = 0;
        }

        Debug.Log(connectedCount);
    }

    private void Update()
    {
        if (!playerSpawned && playerConnected)
        {
            ActivatePlayerSpawn();
        }

        if(!playerConnected)
        {
            if (serverOff <= 0)
            {
                exitWindow.SetActive(true);
            }
            else
            {
                serverOff -= Time.deltaTime;
            }
        }
        
    }

    public void Exit()
    {
        NetworkManager.singleton.StopClient();
        GameObject netObj = FindObjectOfType<NetworkManager>().gameObject;
        Destroy(netObj);
        SceneManager.LoadScene(0);
    }
}

public struct PosMessage : NetworkMessage
{
    public Vector2 vector2;
}