using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class AllErc721Example : MonoBehaviour
{
    private class NFTs
    {
        public string contract { get; set; }
        public string tokenId { get; set; }
        public string uri { get; set; }
        public string balance { get; set; }
    }

    async void Start()
    {
        string chain = "polygon";
        string network = "mainnet"; // mainnet ropsten kovan rinkeby goerli
        string account = "0xD2522633650Ac225eB410D92Eb15Bf82847B0220";
        string contract = "0x4fa6d1Fc702bD7f1607dfeE4206Db368995E1443"; // 0x4fa6d1Fc702bD7f1607dfeE4206Db368995E1443
        int first = 500;
        int skip = 0;
        string response = await EVM.AllErc721(chain, network, account, contract, first, skip);

        Debug.Log(response);

        try
        {
            NFTs[] erc721s = JsonConvert.DeserializeObject<NFTs[]>(response);
            print(erc721s[0].contract);
            print(erc721s[0].tokenId);
            print(erc721s[0].uri);
            print(erc721s[0].balance);
            print("nft count " + erc721s.Length);
        }
        catch
        {
           print("Error: " + response);
        }
    }
}
