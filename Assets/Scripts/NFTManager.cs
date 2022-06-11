using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NFTManager : MonoBehaviour
{
    public class Response {
        public string image;
    }

    async void Start()
    {
        string chain = "polygon";
        string network = "mainnet";
        string contract = "0x4fa6d1Fc702bD7f1607dfeE4206Db368995E1443";
        string tokenId = "149";

        // fetch uri from chain
        string uri = await ERC721.URI(chain, network, contract, tokenId); // change to ERC721
        print("uri: " + uri);

        // fetch json from uri
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        await webRequest.SendWebRequest();
        Response data = JsonUtility.FromJson<Response>(System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data));

        // parse json to get image uri
        string imageUri = data.image;
        print("imageUri: " + imageUri);

        // fetch image and display in game
        UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(imageUri);
        await textureRequest.SendWebRequest();
        Texture2D nft = ((DownloadHandlerTexture)textureRequest.downloadHandler).texture;

        this.gameObject.GetComponent<RawImage>().texture = nft;
    }
}
