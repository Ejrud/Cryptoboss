using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class RegistrationController : MonoBehaviour
{
    [Header("Server")]
    private string altServer = "https://cryptoboss.win/game/back/reg.php"; // �������� ������
    
    [Header("AlertText")]
    [SerializeField] private Text alertText;

    [Header("Inputs")]
    [SerializeField] private InputField emailInput;
    [SerializeField] private InputField userInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private InputField repeatPasswordInput;

    [Header("Other")]
    [SerializeField] private string debugField;
    [SerializeField] private GameObject authWindow;

    private Color defaultColor;
    private Color alertColor;
    private string jsonNft;

    private void Start()
    {
        defaultColor = alertText.color;
        alertColor = Color.red;
    }

    public void PrepareForm()                              // �������� �� ������ ��� � ������
    {
        
            string userEmail = emailInput.text.Trim();
            string userWallet = userInput.text.Trim();
            string userPass = passwordInput.text.Trim();
            string userRepeat = repeatPasswordInput.text.Trim();

            if (!string.IsNullOrWhiteSpace(userWallet) && !string.IsNullOrWhiteSpace(userPass) && !string.IsNullOrWhiteSpace(userEmail) && !string.IsNullOrWhiteSpace(userRepeat))
            {
                if (string.Equals(passwordInput.text, repeatPasswordInput.text))
                {
                   StartCoroutine(GetNft(userWallet, userEmail, userPass)); //GetNft
                }
                else
                {
                    StartCoroutine(SetAlert("Passwords do not match", true));
                    Debug.Log("Passwords do not match");
                }
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(SetAlert("fill in the empty fields", true));
            }
        
    }

    private IEnumerator GetNft(string userWallet, string userEmail, string userPass)
    {  
        Debug.Log("Load nft");
        string uri = "https://cryptoboss.win/ajax/models/messages/customizers/get_nft_by_address_6j986xfw9?address=";
        uri = uri + userWallet;

        // ��������� ���� nft ����� ��������
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
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
                StartCoroutine(SendForm(userWallet, userEmail, userPass, jsonNft)); //   jsonNft
            }
        }
    }

    private IEnumerator SendForm(string userWallet, string userEmail, string userPass, string json)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", userEmail);
        form.AddField("wallet", userWallet);
        form.AddField("pass", userPass);
        form.AddField("data", json);

        // ���������� ������ user_alts � user_profile
        using (UnityWebRequest www = UnityWebRequest.Post(altServer, form)) //    a0664627.xsph.ru/cryptoboss_back/reg.php   //altServer
        { 
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
                StopAllCoroutines();
                StartCoroutine(SetAlert(www.downloadHandler.text));
                if (www.downloadHandler.text != "That wallet already exist")
                {
                    authWindow.SetActive(true);
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }

    private IEnumerator SetAlert(string text = "", bool alert = false) // alert �������� �� ���� ������
    {
        alertText.text = text;
        // alertText.color = (!alert) ? defaultColor : alertColor;

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
