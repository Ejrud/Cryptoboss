using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Web3Api.Models;

public class AuthController : MonoBehaviour
{
    [SerializeField] private bool _debug;
    public enum ServerType {Alt, Sprint, Local}
    [Header("Server")]
    [SerializeField] private ServerType serverType = ServerType.Sprint;
    private string altServer = "https://cryptoboss.win/game/back/auth.php"; // �������� ������
    private string sprintServer = "http://a0664627.xsph.ru/cryptoboss_back/auth.php";
    private string localServer = "http://serverback/auth.php";
    
    [Header("SessionData")]
    [SerializeField] private UserData userData;

    [Header("Inputs")]
    [SerializeField] private InputField inputPass; // Password
    [SerializeField] private GameObject MoralisConnectObj; // android
    [SerializeField] private GameObject ChainsafeConnectObj; // Webgl and ios


    [Header("Meta interface")]
    [SerializeField] private Text metaState;
    [SerializeField] private Text metaId;
    [SerializeField] private string metaConnected = "connected";
    [SerializeField] private string metaDisconnected = "disconnected";
    [SerializeField] private Text alertText;
    private Color defaultColor;
    private Color alertColor;

    private void Start()
    {
        if (_debug)
            inputPass.gameObject.SetActive(true);
        else
            inputPass.gameObject.SetActive(false);

        #if UNITY_WEBGL
            ChainsafeConnectObj.SetActive(true);
            MoralisConnectObj.SetActive(false);
        #endif
        #if UNITY_IOS
            ChainsafeConnectObj.SetActive(true);
            MoralisConnectObj.SetActive(false);
        #endif
        #if UNITY_ANDROID
            ChainsafeConnectObj.SetActive(false);
            MoralisConnectObj.SetActive(true);
        #endif
            
        defaultColor = alertText.color;
        alertColor = Color.red;
    }

    public void PrepareAuth() // For moralis
    {
        Authorization();
    }
    
    public async void Authorization(string wallet = "", bool relog = false) // wallet и relog присваивается при повторном подключении (если у игрока меньше или больше фишек в кошельке чем в игре)
    {
        string chain = "polygon";
        string network = "mainnet"; // mainnet ropsten kovan rinkeby goerli
        string contract = "0x4fa6d1Fc702bD7f1607dfeE4206Db368995E1443"; // 0x4fa6d1Fc702bD7f1607dfeE4206Db368995E1443
        int first = 500;
        int skip = 0;
        string userWallet;

        if (!_debug && !relog)
        {
            StartCoroutine(SetAlert("Authenticating..."));
            #if UNITY_WEBGL
                Debug.Log("ChainSafe connection...");
                userWallet = PlayerPrefs.GetString("Account");
            #endif
            #if UNITY_IOS
                Debug.Log("ChainSafe connection...");
                userWallet = PlayerPrefs.GetString("Account");
            #endif
            #if UNITY_ANDROID
                Debug.Log("Moralis connection...");
                MoralisUser user = await Moralis.GetUserAsync();
                userWallet = user.ethAddress;
            #endif
        }
        else if (relog)
        {
            StartCoroutine(SetAlert("Update chips..."));
            userWallet = wallet;
        }
        else
        {
            StartCoroutine(SetAlert("Debug mode..."));
            userWallet = inputPass.text;
        }

        PlayerPrefs.SetString("Account", userWallet);

        if (!string.IsNullOrWhiteSpace(userWallet))
        {
            if (!userData.IsAuthorized())
            {
                string response = await EVM.AllErc721(chain, network, userWallet, contract, first, skip);

                Debug.Log(response);
                
                StartCoroutine(SendForm(userWallet, response));
                Debug.Log("Sending Form");
            }
            else
            {
                StartCoroutine(SetAlert("You'r authorized", true));
                // Debug.Log("You'r authorized, press f5");
            }
        }
        else
        {
            StartCoroutine(SetAlert("Fill in the empty fields"));
            Debug.Log("Fill in the empty fields");
        }
    }

    private IEnumerator SendForm(string userWallet, string jsonNft)
    {
        WWWForm form = new WWWForm();

        form.AddField("wallet", userWallet);
        form.AddField("UserJson", jsonNft);
        form.AddField("PhantomEmail", (UnityEngine.Random.Range(0, 10000)).ToString());
        string server = localServer;

        switch (serverType)
        {
            case ServerType.Alt:
            server = altServer;
            break;
            case ServerType.Sprint:
            server = sprintServer;
            break;
            case ServerType.Local:
            server = localServer;
            break;
        }
        
        using (UnityWebRequest www = UnityWebRequest.Post(server, form))   // server
        { 
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success && www.downloadHandler.text != "false")
            {
                // Debug.Log("Server response: " + www.downloadHandler.text);

                if (www.downloadHandler.text != "no_chips")
                {
                    string message = www.downloadHandler.text;
                    string[] array = message.Split('|'); // 0 -  2 -  5 -

                    UserDatas userParam = JsonConvert.DeserializeObject<UserDatas>(array[0]);

                    char[] delimiters = {'c', 'h', 'i', 'p', '"', 'C', 'r', 'y', 'p', 't', 'b', 'o', 's', ' ', '#', '}', ',', '{', 'B', ':', '[', ']'};
                    string[] chipIds = array[1].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                    char[] delimiters2 = {'[', ']'};
                    string[] cardJson = array[2].Split(delimiters2, StringSplitOptions.RemoveEmptyEntries);

                    string newJson = "";
                    foreach (string str in cardJson)
                    {
                        newJson += str;
                    }
                    // Debug.Log(newJson);
                    List<DbCards> card = JsonConvert.DeserializeObject<List<DbCards>>("[" + newJson + "]"); // Dictionary<string, string>

                    userData.SetUser(userParam.id, userParam.name, userParam.email, userParam.metamask_wallet, userParam.raiting, chipIds, card, userParam.tutorial);
                    PlayerPrefs.SetString("Wallet", userParam.metamask_wallet);
                    gameObject.SetActive(false);
                }
                else
                {
                   StartCoroutine(SetAlert("No chips found", true)); 
                }
            }
            else
            { 
                StartCoroutine(SetAlert("Server error", true));
                Debug.Log("Server error");
                Debug.Log(www.error);
            }
        }

        yield return null;
    }

    public async void ConnectWallet() // Chainsafe connection
    {
        int timestamp = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
        // set expiration time
        int expirationTime = timestamp + 60;
        // set message
        string message = expirationTime.ToString();
        // sign message
        string signature = await Web3Wallet.Sign(message);
        // verify account
        string account = await EVM.Verify(message, signature);
        int now = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
        // validate
        if (account.Length == 42 && expirationTime >= now) {
            // save account
            PlayerPrefs.SetString("Account", account);
            Authorization();
        }
        else
        {
            StartCoroutine(SetAlert("Wallet Error"));
        }
    }

    public void MoveToRegistration(GameObject regWindow)
    {
        regWindow.SetActive(true);
        gameObject.SetActive(false);
    }

    public class UserDatas
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        [JsonProperty("user_alt_id")]
        public string user_alt_id { get; set; }

        [JsonProperty("metamask_wallet")]
        public string metamask_wallet { get; set; }

        [JsonProperty("media_alt_id")]
        public object media_alt_id { get; set; }

        [JsonProperty("raiting")]
        public string raiting { get; set; }

        [JsonProperty("tutorial")]
        public string tutorial { get; set; }
    }

    private class NFTs
    {
        public string contract { get; set; }
        public string tokenId { get; set; }
        public string uri { get; set; }
        public string balance { get; set; }
    }

    private IEnumerator SetAlert(string text = "", bool alert = false) // alert �������� �� ���� ������
    {
        alertText.text = text;
        alertText.color = (!alert) ? defaultColor : alertColor;

        float apogee = 4f;
        float timer = apogee;

        while (timer > 0)
        {
            alertText.color = new Vector4(alertText.color.r, alertText.color.g, alertText.color.b, timer/apogee);
            timer -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        yield return null;
    }

    private void OnDisable()
    {
        alertText.text = "";
    }
}
