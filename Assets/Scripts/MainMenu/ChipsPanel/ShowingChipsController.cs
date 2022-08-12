using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowingChipsController : MonoBehaviour
{
    [SerializeField] private Tutorial tutorial;
    [SerializeField] private GameObject chipsViewportContent;
    [SerializeField] private GameObject _playBtn;
    [SerializeField] private GameObject _descBtn;

    [Header("Default user")]
    public User user;
    public ChipFrameData[] SelectedChipFrames = new ChipFrameData[2];
    public int Cell = 0;


    [Header("Chip prefab")]
    [SerializeField] private GameObject chipFrame;

    [Header("Links")]
    [SerializeField] private Text chipCapital;
    [SerializeField] private Text chipMorale;
    [SerializeField] private Text chipRating;
    public bool Selecting = false;

    private ChipFrameData[] chipFrameData = new ChipFrameData[0];
    public int SelectedChipID = 0;
    private bool initialize = false;

    private void Start()
    {
        _playBtn.SetActive(false);
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

    public void ResetSelectingMode()
    {
        Selecting = false;
    }

    public void VisiblePlayButton(bool active) // вызывается по нажатию на кнопку
    {
        _playBtn.SetActive(active);
    }

    private void OnEnable()
    {
        if (!initialize)
            ShowChips(user);

        if (!Selecting)
        {
            Debug.Log("Select first chip");
            chipFrameData[0].SelectFirstChip();
            _descBtn.SetActive(true);
        }
        else
        {
            _playBtn.SetActive(false);
            _descBtn.SetActive(false);
            ResetChips();

            user.chipGuid_2 = 0;
            user.chipGuid_3 = 0;

            foreach (ChipFrameData chip in chipFrameData)
            {
                if (chip.chipData.Id == user.chipGuid_1)
                {
                    chip.SelectFirstChip();
                    
                    break;
                }
            }
        }
    }

}


