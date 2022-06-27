using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Accrual : MonoBehaviour  // a0664627.xsph.ru/cryptoboss_back/
{
    private void Start()
    {
        StartCoroutine(SendRequest());
    }

    private IEnumerator SendRequest()
    {
        WWWForm form = new WWWForm();

        using (UnityWebRequest www = UnityWebRequest.Post("a0664627.xsph.ru/cryptoboss_back/accural.php", form))
        { 
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                Debug.Log(json);
            }
            else
            { 
                Debug.Log("Incorrect data");
                Debug.Log(www.error);
            }
        }
    }
}
