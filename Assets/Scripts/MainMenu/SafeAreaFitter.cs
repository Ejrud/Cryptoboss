using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private Rect currentRect;

    private void Awake()
    {
        SafeAreaFit();
    }

    private void SafeAreaFit()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Rect safeArea = Screen.safeArea;
        currentRect = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = anchorMin + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }

    private void Update()
    {
        if (Screen.safeArea != currentRect)
        {
            SafeAreaFit();
        }
    }

}