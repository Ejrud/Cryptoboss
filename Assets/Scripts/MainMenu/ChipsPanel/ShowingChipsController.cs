using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowingChipsController : MonoBehaviour
{
    [SerializeField] private GameObject chipsViewportContent;

    [Header("Default user")]
    [SerializeField] private User user;

    [Header("Chip prefab")]
    [SerializeField] private GameObject chipFrame;

    private void Start()
    {
        ShowChips(user); // ѕри авторизации пользовател€ вводить иной scriptableObject с заранее загруженными данными
    }

    public void ShowChips(User user)
    {
        for (int i = 0; i < user.chipDatas.Length; i++)
        {
            GameObject chipFrame = Instantiate(this.chipFrame, new Vector3(0, 0, 0), Quaternion.identity);

            chipFrame.transform.SetParent(chipsViewportContent.transform);
            chipFrame.GetComponent<ChipFrameData>().Init(user.chipDatas[i], false);
            chipFrame.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }
    }
}


