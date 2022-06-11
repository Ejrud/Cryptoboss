using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeGraphics : MonoBehaviour
{
    [SerializeField] private Image[] buttonImages;

    private Color defaultColor = Color.white;

    public void ChangeGfx(int index)
    {
        foreach (Image img in buttonImages)
        { 
            img.color = defaultColor;
        }

        QualitySettings.SetQualityLevel(index, true); //Change gfx 0 - low, 1 - mid, 2 - high
    }
}
