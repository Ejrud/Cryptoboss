using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

public class UpdateBalance : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private User _user;
    [SerializeField] private Text _balanceText;

    private bool _loading;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_loading)
        {
            Debug.Log("Get balance...");
            UpdateBal();
        }
    }

    private async void UpdateBal()
    {
        _loading = true;

        string uri = $"https://cryptoboss.win/ajax/models/comments/customizers/get_user_balance_ixdkznne7?address={_user.Wallet}";

        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        
        // Request and wait for the desired page.
        await webRequest.SendWebRequest();

        string[] pages = uri.Split('/');
        int page = pages.Length - 1;

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            UserBalance Balance = JsonConvert.DeserializeObject<UserBalance>(webRequest.downloadHandler.text);
            _user.Balance = Balance.balance;

            string balance = _user.Balance;
            int zIndex = 0;
            for (int i = 0; i < balance.Length; i++)
            {
                if (balance[i] == '.')
                {
                    Debug.Log("Balance: " + balance[i]);
                    zIndex = i+3;
                    if (zIndex < balance.Length)
                        balance = balance.Remove(zIndex);

                    break;
                }
            }

            _balanceText.text = balance;
            Debug.Log($"Balance: {balance}");
        }

        _loading = false;
    }
    public class UserBalance
    {
        public string balance { get; set;}
    }
}
