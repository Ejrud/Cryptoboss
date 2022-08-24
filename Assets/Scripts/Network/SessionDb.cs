using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class SessionDb : MonoBehaviour
{
    private Session _currentSession;
    private string url = "a0664627.xsph.ru/cryptoboss_back/sessionManager.php";

    public void CreateSession(Session session)
    {
        StartCoroutine(Create(session));
    }

    public void LeaveSession()
    {
        StartCoroutine(Leave());
    }

    public void CompleteSession(List<int> chipIds, List<float> bossyReward, List<int> raitingReward)
    {
        StartCoroutine(Complete(chipIds, bossyReward, raitingReward));
    }

    ///////////////////

    private IEnumerator Create(Session session)
    {
        _currentSession = session;

        SessionData sessionData = new SessionData();
        sessionData.id = "0";
        sessionData.action = "Create";
        sessionData.name = _currentSession.PlayerNets[0].GameMode; // режим сессии

        List<ChipData> chipData = new List<ChipData>();

        for (int i = 0; i < _currentSession.PlayerNets.Length; i++)
        {
            ChipData chip = new ChipData();

            chip.guid = "CryptoBoss #" + _currentSession.PlayerNets[i].ChipId;
            chip.rating = "";
            chip.bossy = "";

            chipData.Add(chip);
        }

        sessionData.chips = chipData;
        string jsonData = JsonConvert.SerializeObject(sessionData);

        StartCoroutine(SendForm(jsonData, "Create"));

        yield return null;
    }

    private IEnumerator Leave()
    {
        SessionData sessionData = new SessionData();
        sessionData.id = _currentSession.Id;
        sessionData.action = "Leave";
        sessionData.name = _currentSession.GameMode; // режим сессии

        List<ChipData> chipData = new List<ChipData>();

        for (int i = 0; i < _currentSession.StatsHolder.Count; i++)
        {
            for (int j = 0; j < _currentSession.StatsHolder[i].ChipId.Length; j++)
            {
                ChipData chip = new ChipData();

                chip.guid = "CryptoBoss #" + _currentSession.StatsHolder[i].ChipId[j];
                chip.rating = "8";
                chip.bossy = "0";

                chipData.Add(chip);
            }
        }

        sessionData.chips = chipData;
        string jsonData = JsonConvert.SerializeObject(sessionData);

        StartCoroutine(SendForm(jsonData, "Leave"));
        
        yield return null;
    }

    private IEnumerator Complete(List<int> chipIds, List<float> bossyReward, List<int> raitingReward) // первый индекс победитель, второй проигравший (2 на 2 - первый и второй победители)
    {
        SessionData sessionData = new SessionData();
        sessionData.id = _currentSession.Id;
        sessionData.action = "Complete";
        sessionData.name = _currentSession.GameMode; // режим сессии

        List<ChipData> chipData = new List<ChipData>();

        for (int i = 0; i < chipIds.Count; i++)
        {
            ChipData chip = new ChipData();
            chip.guid = "CryptoBoss #" + chipIds[i];
            chip.bossy = bossyReward[i].ToString();
            chip.rating = raitingReward[i].ToString();

            chipData.Add(chip);
        }

        sessionData.chips = chipData;
        string jsonData = JsonConvert.SerializeObject(sessionData);

        StartCoroutine(SendForm(jsonData, "Complete"));

        yield return null;
    }

    private IEnumerator SendForm(string json, string action)
    {
        WWWForm form = new WWWForm();
        form.AddField("JsonData", json);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form)) // Запись сессии
        { 
            yield return www.SendWebRequest();
            
            if (action == "Create")
            {
                if (www.result == UnityWebRequest.Result.Success)
                {
                    _currentSession.Id = www.downloadHandler.text;
                    Debug.Log(_currentSession.Id);
                }
                else
                {
                    _currentSession.Id = "null";
                }
            }

            yield return null;
        }

        if (action != "Create")
            _currentSession.DBUpdated = true;
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
