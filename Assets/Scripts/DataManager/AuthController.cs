using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class AuthController : MonoBehaviour
{
    public enum ServerType {Alt, Sprint, Local}
    [Header("Server")]
    [SerializeField] private ServerType serverType = ServerType.Sprint;
    private string altServer = "https://cryptoboss.win/game/back/auth.php"; // Изменить адресс
    private string sprintServer = "http://a0664627.xsph.ru/cryptoboss_back/auth.php";
    private string localServer = "http://serverback/auth.php";
    
    [Header("SessionData")]
    [SerializeField] private UserData userData;

    [Header("Inputs")]
    [SerializeField] private InputField inputMetaMask; // Login
    [SerializeField] private InputField inputPass; // Password


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
        defaultColor = alertText.color;
        alertColor = Color.red;

        UpdateMetaState();
    }

    
    public void PrepareAuth()
    {
        string userWallet = inputMetaMask.text.Trim();
        string userPass = inputPass.text.Trim();

        if (!string.IsNullOrWhiteSpace(userWallet))
        {
            if (!userData.IsAuthorized())
            {
                    StartCoroutine(GetNft(userWallet));
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

    public void UpdateMetaState()
    {
        if (PlayerPrefs.GetString("Account") != "")
        {
            metaState.text = metaConnected;
            metaId.text = PlayerPrefs.GetString("Account");
        }
        else
        {
            metaState.text = metaDisconnected;
            metaId.text = "";
        }
    }

    private IEnumerator GetNft(string userWallet)
    {  
        Debug.Log("Load nft");
        string uri = "https://cryptoboss.win/ajax/models/messages/customizers/get_nft_by_address_6j986xfw9?address=";
        uri = uri + userWallet;

        // Получение всех nft этого кошелька
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            string jsonNft = "";
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(webRequest.downloadHandler.text);
                    jsonNft = webRequest.downloadHandler.text;
                    break;
            }
            

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                StartCoroutine(SendForm(userWallet, jsonNft)); //   jsonNft
            }
        }
    }

    private IEnumerator SendForm(string userWallet, string jsonNft)           // отправка формы на хостинг
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
        
        //Отправление запроса на Altrp сервер
        using (UnityWebRequest www = UnityWebRequest.Post(server, form))   // server
        { 
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            // www.SetRequestHeader("Content-Type", "application/json");  Не рекомендуется использовать
            // www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36"); // !!!

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success && www.downloadHandler.text != "false")
            {
                Debug.Log("Server response: " + www.downloadHandler.text);

                string message = www.downloadHandler.text;
                string[] array = message.Split('|'); // 0 - данные пользователя, 2 - все его фишки, 5 - данные карт // , '[', ']'

                // Создание массива параметров пользователя
                UserDatas userParam = JsonConvert.DeserializeObject<UserDatas>(array[0]);
                // Debug.Log(userParam.name);

                // Создание массива id фишек
                char[] delimiters = {'c', 'h', 'i', 'p', '"', 'C', 'r', 'y', 'p', 't', 'b', 'o', 's', ' ', '#', '}', ',', '{', 'B', ':', '[', ']'};
                string[] chipIds = array[1].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                foreach (string id in chipIds)
                {
                    Debug.Log(id);
                }

                // Создание массива карт для одной фишки
                char[] delimiters2 = {'[', ']'};
                string[] cardJson = array[2].Split(delimiters2, StringSplitOptions.RemoveEmptyEntries);

                string newJson = "";
                foreach (string str in cardJson)
                {
                    newJson += str;
                }
                Debug.Log(newJson);
                List<DbCards> card = JsonConvert.DeserializeObject<List<DbCards>>("[" + newJson + "]"); // Dictionary<string, string>

                userData.SetUser(userParam.id, userParam.name, userParam.email, userParam.metamask_wallet, userParam.raiting, chipIds, card);
                PlayerPrefs.SetString("Wallet", userParam.metamask_wallet);
                gameObject.SetActive(false);
            }
            else
            { 
                StartCoroutine(SetAlert("Incorrect login or password", true));
                Debug.Log("Incorrect login or password");
                Debug.Log(www.error);
            }
        }

        yield return null;
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

    private IEnumerator SetAlert(string text = "", bool alert = false) // alert отвечает за цвет текста
    {
        alertText.text = text;
        // alertText.color = (!alert) ? defaultColor : alertColor;

        float apogee = 4f;
        float timer = apogee;

        while (timer > 0)
        {
            alertText.color = new Vector4(alertText.color.r, alertText.color.g, alertText.color.b, timer/apogee);
            timer -= Time.deltaTime;
            yield return new WaitForUpdate();
        }

        yield return null;
    }

    private void OnDisable()
    {
        alertText.text = "";
    }
}
