using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipContainer : MonoBehaviour
{
    public int ChipId;
    public string ChipName;

    public void Init(int ChipId, string ChipName)
    {
        this.ChipId = ChipId;
        this.ChipName = ChipName;
    }
}
