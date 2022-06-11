using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GraphicsButton : MonoBehaviour, IPointerDownHandler
{
    [Header("Quality index (0 - low, 1 - mid, 2 - high)")]
    [SerializeField] private int index = 0;

    [SerializeField] private ChangeGraphics changeGraphics;
    [SerializeField] private Color activeColor = Color.cyan;

    private Image currentImage;

    private void Start()
    {
        currentImage = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        changeGraphics.ChangeGfx(index);
        currentImage.color = activeColor;
    }
}
