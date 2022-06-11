using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UserManager : MonoBehaviour
{
    [SerializeField] string chain = "polygon";
    [SerializeField] string network = "mainnet";
    [SerializeField] private string userContractAdress; // contract address выводится в массиве всех nft пользователя  0x4fa6d1Fc702bD7f1607dfeE4206Db368995E1443
    [SerializeField] private string[] tokenIds; // колличество nft

    private Texture[] nftTextures;

    private void Start()
    {
        GetNftAsync();
    }

    public async void GetNftAsync()
    {
        nftTextures = new Texture[tokenIds.Length];

        for (int i = 0; i < tokenIds.Length; i++)
        {
            string uri = await ERC721.URI(chain, network, userContractAdress, tokenIds[i]);
            print("uri: " + uri);

            UnityWebRequest webRequest = UnityWebRequest.Get(uri);
            await webRequest.SendWebRequest();
            Response data = JsonUtility.FromJson<Response>(System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data));

            string imageUri = data.image;
            print("imageUri: " + imageUri);

            UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(imageUri);
            await textureRequest.SendWebRequest();
            nftTextures[i] = ((DownloadHandlerTexture)textureRequest.downloadHandler).texture;
        }

        GlobalEventManager.OnLoadNftTexture.Invoke(nftTextures);
    }

    public Texture[] GetNftTexture()
    {
        return nftTextures;
    }

    public class Response
    {
        public string image;
    }
}


