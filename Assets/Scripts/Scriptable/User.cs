using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "UserData")]
public class User : ScriptableObject
{
    public string Balance;
    public bool Authorized;
    public bool Tutorial;
    public string UserID;
    public string UserName;
    public string Email;
    public string Wallet;
    public string Score;
    public bool Mute;

    [Tooltip("Chip id in array of chips")]
    public int SelectedChip; // индекс в массиве с фишками
    public int chipGuid_1;
    public int chipGuid_2;
    public int chipGuid_3;

    // debug zone
    public List<ChipParameters> ChipParam;
    public string[] nftTokens;
    public List<DbCards> cards;
}