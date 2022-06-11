using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "UserData")]
public class User : ScriptableObject
{
    public bool Authorized;
    public string UserID;
    public string UserName;
    public string Email;
    public string Wallet;
    public string Score;

    // debug zone
    public ChipData[] chipDatas;
    public string[] nftTokens;
    public List<DbCards> cards;
}