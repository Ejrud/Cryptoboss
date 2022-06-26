using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SelectChip : MonoBehaviour
{
    [Header("All chips")]
    [SerializeField] private User user;
    [SerializeField] private GameObject selectableChipPrefab;

    [Header("UI")]
    [SerializeField] private Transform[] chipPositions = new Transform[3];
    [SerializeField] private Transform chipContainer;
    [SerializeField] private Text ChipName;

    private GameObject[] selectableChips;
    private int[] currentIdPos;
    private bool chipsLoaded;
    private int offsetMassive = 0;

    // «агрузка всех фишек пользовател€ в главное меню
    public void Init(List<ChipParameters> chipParam)
    {
        if (selectableChips != null)
        {
            foreach (GameObject chipObj in selectableChips)
            {
                Destroy(chipObj);
            }
        }

        selectableChips = new GameObject[chipParam.Count];

        currentIdPos = new int[chipParam.Count];

        if(currentIdPos.Length > 3)
        {
            currentIdPos = new int[3];
        }

        for (int i = 0; i < currentIdPos.Length; i++)
        {
            currentIdPos[i] = i;
        }

        for (int i = 0; i < chipParam.Count; i++)
        {
            selectableChips[i] = Instantiate(selectableChipPrefab, Vector3.zero, Quaternion.identity);

            selectableChips[i].transform.SetParent(chipContainer);
            selectableChips[i].transform.localScale = new Vector3(1,1,1);
            selectableChips[i].GetComponent<RawImage>().texture = chipParam[i].ChipTexture;
            selectableChips[i].GetComponent<ChipContainer>().Init(chipParam[i].Id, chipParam[i].ChipName);

            selectableChips[i].SetActive(false);
        }

        chipsLoaded = true;

        UpdatePositions();
    }

    public void NextChip()
    {
        for (int i = 0; i < currentIdPos.Length; i++)
        {
            currentIdPos[i]++;
            if (currentIdPos[i] > selectableChips.Length - 1)
            {
                currentIdPos[i] = currentIdPos[i] - selectableChips.Length;
            }
        }
        UpdatePositions();
    }

    public void PreviousChip()
    {
        for (int i = 0; i < currentIdPos.Length; i++)
        {
            currentIdPos[i]--;
            if (currentIdPos[i] < 0)
            {
                currentIdPos[i] = selectableChips.Length + currentIdPos[i];
            }
        }
        UpdatePositions();
    }

    private void UpdatePositions()
    {
        foreach (GameObject chipObj in selectableChips)
        {
            chipObj.SetActive(false);
            chipObj.transform.localScale = new Vector3(1,1,1);
        }

        int count = 0;

        for (int i = 0; i < currentIdPos.Length; i++)
        {
            selectableChips[currentIdPos[i]].transform.position = chipPositions[i].position;
            selectableChips[currentIdPos[i]].SetActive(true);

            if(count == 1)
            {
                selectableChips[currentIdPos[i]].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                int chipId = Convert.ToInt32(selectableChips[currentIdPos[i]].GetComponent<ChipContainer>().ChipId);
                string chipName = selectableChips[currentIdPos[i]].GetComponent<ChipContainer>().ChipName;

                PlayerPrefs.SetInt("chipId", chipId);
                Debug.Log("Selected chipId: " + chipId);

                ChipName.text = chipName;
            }
            count++;
        }

        if (selectableChips.Length == 1)
        {
            selectableChips[0].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            selectableChips[0].transform.position = chipPositions[1].transform.position;

            int chipId = Convert.ToInt32(selectableChips[0].GetComponent<ChipContainer>().ChipId);
            string chipName = selectableChips[0].GetComponent<ChipContainer>().ChipName;

            PlayerPrefs.SetInt("chipId", chipId);
            Debug.Log("Selected chipId: " + chipId);

            ChipName.text = chipName;
        }
    }
}