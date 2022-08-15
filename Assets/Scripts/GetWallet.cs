using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Web3Api.Models;
using UnityEngine.SceneManagement;

public class GetWallet : MonoBehaviour
{
    private void Start()
    {
        Connect();
    }

    public async void Connect()
    {
        string response = await Web3Wallet.Sign("hello");
        print(response);
    }
}
