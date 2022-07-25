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

    [Header("Slider settings")]
    [SerializeField] private float offset = 2.5f;
    [SerializeField] private float timeToStop = 2f;
    [SerializeField] private  SlidingChips slidingChip;

    [Header("UI")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform center;
    [SerializeField] private Transform chipContainer;
    [SerializeField] private Text ChipName;
    [SerializeField] private Text ChipMorale;
    [SerializeField] private Button playButton;

    private GameObject[] selectableChips = new GameObject[0];
    private GameObject selectedChip;
    private bool chipsLoaded;
    private bool stabilize = false;
    private bool move;
    

    private int currentChipIndex = 0;

    private float timer = 0;

    // �������� ���� ����� ������������ � ������� ����
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

        for (int i = 0; i < chipParam.Count; i++)
        {
            selectableChips[i] = Instantiate(selectableChipPrefab, Vector3.zero, Quaternion.identity);

            selectableChips[i].transform.SetParent(chipContainer);
            selectableChips[i].transform.localScale = new Vector3(1,1,1);
            selectableChips[i].GetComponent<RawImage>().texture = chipParam[i].ChipTexture;
            selectableChips[i].GetComponent<ChipContainer>().Init(chipParam[i].Id, chipParam[i].ChipName, chipParam[i].Morale);

            // Если у фишки будет недостаточно энергии для игры то она помечается серым (данные обновляются при перезаходе в игру или возврате из игровой сессии)
            if (chipParam[i].Morale == "0") 
            {
                Debug.Log("0");
                selectableChips[i].GetComponent<RawImage>().color = Color.gray;
            }
            else
            {
                selectableChips[i].GetComponent<RawImage>().color = Color.white;
            }
        }
        
        timer = 0;
        stabilize = true;
        scrollRect.velocity = new Vector2(10000, 0); // узнать сколько силы нужно дать для перемещения на 1 фишку
    }

    private void Update()
    {
        if (selectableChips.Length > 0)
        {
            if (Input.GetMouseButton(0)) //  && slidingChip.AcceptMove
            {
                timer = timeToStop;
                stabilize = true;
            }

            if (timer <= 0 && scrollRect.velocity.magnitude < 100f && stabilize)
            {
                StartCoroutine(Stabilize());
            }

            for (int i = 0; i < selectableChips.Length; i++)
            {
                // Если фишка не входит в проомежУток, то скипается
                if (selectableChips[i].transform.position.x < center.position.x - offset || selectableChips[i].transform.position.x > center.position.x + offset)
                {
                    selectableChips[i].transform.localScale = new Vector3(1,1,1);
                    continue;
                }

                float scaleMultiply = 1;
                float currentScale = 1;

                float currentOffset = selectableChips[i].transform.position.x - center.transform.position.x; // offset
                currentOffset = (currentOffset > 0) ? currentOffset : currentOffset * -1;
                currentOffset = offset - currentOffset;
                currentScale *= scaleMultiply + (currentOffset / 10);

                selectableChips[i].transform.localScale = new Vector3(currentScale,currentScale,currentScale);

                if (selectableChips[i].transform.position.x > center.position.x - offset / 2 && selectableChips[i].transform.position.x < center.position.x + offset / 2)
                {
                    int chipId = Convert.ToInt32(selectableChips[i].GetComponent<ChipContainer>().ChipId);
                    string chipName = selectableChips[i].GetComponent<ChipContainer>().ChipName;
                    string chipMorale = selectableChips[i].GetComponent<ChipContainer>().Morale;

                    if (chipName != ChipName.text)
                    {
                        int morale = Convert.ToInt32(selectableChips[i].GetComponent<ChipContainer>().Morale);

                        if (morale <= 0)
                        {
                            playButton.interactable = false;
                        }
                        else
                        {
                            playButton.interactable = true;
                        }

                        PlayerPrefs.SetInt("chipId", chipId);
                        selectedChip = selectableChips[i];
                        currentChipIndex = i;
                        Debug.Log(currentChipIndex);
                        // Debug.Log("Selected chipId: " + chipId);
                        // Debug.Log("Morale updated");
                    }

                    ChipName.text = chipName;
                    ChipMorale.text = chipMorale;
                }
            }

            timer = (timer <= 0) ? timer : timer -= Time.deltaTime;
        }
    }
    public void NextChip()
    {
        currentChipIndex++;
        if (currentChipIndex > selectableChips.Length - 1)
        {
            currentChipIndex = selectableChips.Length - 1;
        }
        else
        {
            selectedChip = selectableChips[currentChipIndex];
            stabilize = true;
            timer = 0;
        }

        Debug.Log(currentChipIndex);
    }

    public void PreviousChip()
    {
        currentChipIndex--;
        if (currentChipIndex < 0)
        {
            currentChipIndex = 0;
        }
        else
        {
            selectedChip = selectableChips[currentChipIndex];
            stabilize = true;
            timer = 0;
        }

        Debug.Log(currentChipIndex);
    }

    private IEnumerator Stabilize()
    {
        scrollRect.StopMovement();

        float step = 0;
        float offset = center.transform.position.x - selectedChip.transform.position.x;
        float supposedPosition = chipContainer.transform.position.x + offset;
        bool direction = (offset > 0) ? true : false; // true - right, false - left
        
        step = offset / 100;

        move = true;

        while (move)
        {
            chipContainer.transform.position += new Vector3(step, 0,0);

            if (direction && chipContainer.transform.position.x >= supposedPosition) // right
            {
                chipContainer.transform.position = new Vector3(supposedPosition, chipContainer.transform.position.y, chipContainer.transform.position.z);
                move = false;
            }
            else if (!direction && chipContainer.transform.position.x <= supposedPosition)
            {
                chipContainer.transform.position = new Vector3(supposedPosition, chipContainer.transform.position.y, chipContainer.transform.position.z);
                move = false;
            }

            yield return new WaitForSeconds(.01f);
        }

        stabilize = false;

        yield return null;
    }

}