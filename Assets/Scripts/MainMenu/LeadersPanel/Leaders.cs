using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using Org.BouncyCastle.Bcpg;

public class Leaders : MonoBehaviour
{
    [Header("Chips")]
    [SerializeField] private RawImage[] chipImage = new RawImage[3];

    [Header("Leaders table")]
    [SerializeField] private Text[] nameRows = new Text[10];
    [SerializeField] private Text[] scoreRows = new Text[10];
    [SerializeField] private GameObject[] _rows = new GameObject[10];

    [SerializeField] private Text[] _chipNames = new Text[3];

    private List<LeadersDb> _leaders = new List<LeadersDb>();
    private string[] _chipIds;
    
    private string seUrl = "https://cryptoboss.win/game/back/"; // http://a0664627.xsph.ru/cryptoboss_back/      // https://cryptoboss.win/game/back/

    private void Start()
    {
        foreach (RawImage image in chipImage)
        {
            image.gameObject.SetActive(false);
        }

        foreach (Text txt in _chipNames)
        {
            txt.gameObject.SetActive(false);
        }

        UpdateLeaderBoard();
        StartCoroutine(GetLeaders());
    }
    // �������� �������
    private IEnumerator GetLeaders()
    {
        WWWForm form = new WWWForm();
        
        using (UnityWebRequest www = UnityWebRequest.Post(seUrl + "get_leaders.php", form))
        { 
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;

                _leaders = JsonConvert.DeserializeObject<List<LeadersDb>>(json);

                GetImages();
                UpdateLeaderBoard();
            }
            else
            { 
                Debug.Log("Incorrect data");
                Debug.Log(www.error);
            }
        }

        yield return null;
    }
    // �������� �����������
    private async void GetImages()
    {
        char[] delimiters = {'c', 'h', 'i', 'p', '"', 'C', 'r', 'y', 'p', 't', 'b', 'o', 's', ' ', '#', '}', ',', '{', 'B', ':', '[', ']'};

        for (int i = 0; i < 3; i++)
        {
            if (i < _leaders.Count)
            {
                string[] chipIndex = _leaders[i].guid.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(seUrl + "images/" + chipIndex[0] + ".png");
                await textureRequest.SendWebRequest();
                Texture2D nft = ((DownloadHandlerTexture)textureRequest.downloadHandler).texture;
                
                chipImage[i].gameObject.SetActive(true);
                _chipNames[i].gameObject.SetActive(true);

                chipImage[i].texture = nft;
                _chipNames[i].text = _leaders[i].guid;
            }
            else
            {
                chipImage[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetLeadersOfWeek() // ���������� �������� "�����" "������"
    {
        // UpdateLeaderBoard(namesOfWeek, scoreOfWeek);
    }

    public void SetLeadersOfSeason()
    {
        // UpdateLeaderBoard(namesOfSeason, scoreOfSeason);
    }
    
    private void UpdateLeaderBoard()
    {
        for (int i = 0; i < nameRows.Length; i++)
        {
            _rows[i].SetActive(false);
        }

        for (int i = 0; i < _leaders.Count; i++)
        {
            _rows[i].SetActive(true);
            
            nameRows[i].text = _leaders[i].guid;
            scoreRows[i].text = _leaders[i].rating;
        }
    }
    
    public class LeadersDb
    {
        public string guid { get; set; }
        public string rating { get; set; }
    }
}
