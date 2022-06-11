using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

public class DbConnection : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(SendForm());
    }
    private IEnumerator SendForm()           // отправка формы на хостинг
    {
        WWWForm form = new WWWForm();
        // form.AddField("wallet", userWallet);
        // form.AddField("pass", userPass);

        string uri = "https://cryptoboss.win/ajax/models/messages/customizers/get_nft_by_owner_q6ftved3w";
        //Отправление запроса на сервер
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))   // https://ajax/models/users_sites/customizers/create_user_by_metamask_nm9dbu0nn?
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
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
        
        yield return null;
    }
}
