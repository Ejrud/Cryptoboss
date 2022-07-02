using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipContainer : MonoBehaviour
{
    public int ChipId;
    public string ChipName;
    public string Morale;

    public void Init(int ChipId, string ChipName, string Morale)
    {
        this.ChipId = ChipId;
        this.ChipName = ChipName;
        this.Morale = Morale;
    }
}
