using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowingChipsController : MonoBehaviour
{
    [SerializeField] private Tutorial tutorial;
    [SerializeField] private GameObject chipsViewportContent;

    [Header("Default user")]
    [SerializeField] private User user;

    [Header("Chip prefab")]
    [SerializeField] private GameObject chipFrame;

    [Header("Links")]
    [SerializeField] private Text chipCapital;
    [SerializeField] private Text chipMorale;
    [SerializeField] private Text chipRating;

    private ChipFrameData[] chipFrameData = new ChipFrameData[0];
    public int SelectedChipID = 0;
    private bool initialize = false;

    private void Start()
    {
        ShowChips(user); // ��� ����������� ������������ ������� ���� scriptableObject � ������� ������������ �������
    }

    public void ShowChips(User user)
    {
        chipFrameData = new ChipFrameData[user.ChipParam.Count];
        for (int i = 0; i < user.ChipParam.Count; i++)
        {
            GameObject chipFrame = Instantiate(this.chipFrame, new Vector3(0, 0, 0), Quaternion.identity);

            chipFrame.transform.SetParent(chipsViewportContent.transform);
            chipFrame.GetComponent<ChipFrameData>().Init(this, user.ChipParam[i], chipCapital, chipMorale, chipRating, i, false);
            chipFrame.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            chipFrameData[i] = chipFrame.GetComponent<ChipFrameData>();
        }

        chipFrameData[0].SelectFirstChip();
        initialize = true;
    }

    public void ResetChips(bool first = false)
    {
        if (chipFrameData.Length > 0)
        {
            foreach (ChipFrameData chip in chipFrameData)
            {
                chip.Reset(first);
            }
        }
    }

    private void OnEnable()
    {
        ResetChips(true);
        if (initialize)
            chipFrameData[SelectedChipID].SelectFirstChip();
    }
}


