using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startClientButton;

    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject clientCountButton;

    //[SerializeField] private GameManager gameManagerScript;

    //[SerializeField] private GameObject menuCamera;
    // Start is called before the first frame update
    void Start()
    {
        //gameManagerScript = GameObject.Find("/GameManager").GetComponent<GameManager>();

        // PUN!!!

        //startHostButton.onClick.AddListener(() => { NetworkManager.Singleton.StartHost();  OnSuccessConnection(); });
        //startServerButton.onClick.AddListener(() => { NetworkManager.Singleton.StartServer(); OnSuccessConnection(); ServerUpdate(); });
        //startClientButton.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); OnSuccessConnection(); GameManagerUpdate(); });
    }

    private void ServerUpdate()
    {
            print("Host started");
            clientCountButton.SetActive(true);
    }

    private void GameManagerUpdate()
    {
            clientCountButton.SetActive(false);

            print("Client started");
    }

    private void OnSuccessConnection()
    {
        buttonsPanel.SetActive(false);
        mainCanvas.SetActive(true);
        //menuCamera.SetActive(false);
    }

    public void PanelActive(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }
}

