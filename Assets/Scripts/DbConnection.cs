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
        SessionData sessionData = new SessionData();
        sessionData.id = "1";
        sessionData.action = "Leave";
        sessionData.name = "one"; // режим сессии

        List<ChipData> chipData = new List<ChipData>();

        for (int i = 1; i < 3; i++)
        {
            ChipData chip = new ChipData();

            chip.guid = "CryptoBoss #" + i;
            chip.rating = "123";
            chip.bossy = "12";

            chipData.Add(chip);
        }

        sessionData.chips = chipData;

        string jsonData = JsonConvert.SerializeObject(sessionData);
        Debug.Log(jsonData);

        StartCoroutine(SendForm(jsonData));
    }
    private IEnumerator SendForm(string json)
    {
        string url = "a0664627.xsph.ru/cryptoboss_back/sessionManager.php";
        WWWForm form = new WWWForm();
        form.AddField("JsonData", json);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.downloadHandler.text);
        }

        yield return null;
    }

    [System.Serializable]
    public class SessionData // id, action, name, chips (хранит guid, рейтинг, bossy)
    {
        public string id; 
        public string action; 
        public string name; 
        public List<ChipData> chips;
    }

    [System.Serializable]
    public class ChipData
    {
        public string guid;
        public string rating;
        public string bossy;
    }
}
