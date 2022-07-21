using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] private User user;
    [SerializeField] private UserData userData;
    [SerializeField] private int nameLength = 18;

    [Header("UI")]
    [SerializeField] private InputField userName;
    [SerializeField] private InputField userWallet;
    [SerializeField] private Text bossyRaiting;

    private string editUrl = "https://cryptoboss.win/game/back/editProfile.php";

    public void UpdateUser()
    {
        if (!string.IsNullOrWhiteSpace(userName.text) || !string.IsNullOrWhiteSpace(userWallet.text))
        {
            if (userName.text != user.UserName || userWallet.text != user.Wallet)
            {
                StartCoroutine(SendForm());
            }
            
        }
        else
        {
            Debug.Log("Fill in the empty fields");
        }
    }

    private IEnumerator SendForm()
    {
        WWWForm form = new WWWForm();

        string name = userName.text;

        if (name.Length > nameLength)
        {
            name = name.Remove(nameLength);
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            yield return null;
        }
        
        form.AddField("UserName", name);
        form.AddField("UserWallet", user.Wallet);
        form.AddField("Id", Convert.ToInt32(user.UserID));

        using (UnityWebRequest www = UnityWebRequest.Post(editUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                user.UserName = name;
                userData.UpdateUI();
                Debug.Log("Name or wallet is changed");
                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                Debug.Log(www.error);
            }
        }

        yield return null;
    }

    // ��� ��������� ������� ����������� ����
    private void OnEnable()
    {
        userName.text = user.UserName;
        userWallet.text = user.Wallet;
        bossyRaiting.text = user.Score;
    }
}
