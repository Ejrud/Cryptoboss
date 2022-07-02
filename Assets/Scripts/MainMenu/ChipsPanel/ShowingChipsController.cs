using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowingChipsController : MonoBehaviour
{
    [SerializeField] private GameObject chipsViewportContent;

    [Header("Default user")]
    [SerializeField] private User user;

    [Header("Chip prefab")]
    [SerializeField] private GameObject chipFrame;

    [Header("Links")]
    [SerializeField] private Text chipCapital;
    [SerializeField] private Text chipMorale;
    [SerializeField] private Text chipRating;

    private void Start()
    {
        ShowChips(user); // ��� ����������� ������������ ������� ���� scriptableObject � ������� ������������ �������
    }

    public void ShowChips(User user)
    {
        for (int i = 0; i < user.ChipParam.Count; i++)
        {
            GameObject chipFrame = Instantiate(this.chipFrame, new Vector3(0, 0, 0), Quaternion.identity);

            chipFrame.transform.SetParent(chipsViewportContent.transform);
            chipFrame.GetComponent<ChipFrameData>().Init(user.ChipParam[i], chipCapital, chipMorale, chipRating, false);
            chipFrame.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }
    }
}


