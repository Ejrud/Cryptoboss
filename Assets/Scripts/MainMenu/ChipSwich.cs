using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipSwich : MonoBehaviour
{
    [SerializeField] private GameObject[] Chips = new GameObject[3];
    [SerializeField] private Vector2[] NewPose = new Vector2[3];
    [SerializeField] private Vector2 BigSizeChip;
    [SerializeField] private Vector2 LittleSizeChip;

    // Start is called before the first frame update
    void Start()
    {
        BigSizeChip = Chips[1].GetComponent<RectTransform>().sizeDelta;
        LittleSizeChip = Chips[0].GetComponent<RectTransform>().sizeDelta;
        //for(int i = 0; i < Chips.Length; i++)
        //{
        //    NewChips[i] = Chips[i].transform.position;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChipSwichRight()
    {
        NewPose[0] = Chips[1].transform.position;
        NewPose[1] = Chips[2].transform.position;
        NewPose[2] = Chips[0].transform.position;

        ChipSwichMethod();
    }

    public void ChipSwichLeft()
    {
        NewPose[2] = Chips[1].transform.position;
        NewPose[1] = Chips[0].transform.position;
        NewPose[0] = Chips[2].transform.position;

        ChipSwichMethod();
    }

    private void ChipSwichMethod()
    {
        for(int i = 0; i < Chips.Length; i++)
        {
            Chips[i].transform.localScale = new Vector2(1, 1);
        }

        Chips[0].transform.position = NewPose[0];
        Chips[1].transform.position = NewPose[1];
        Chips[2].transform.position = NewPose[2];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.transform.localScale = new Vector2(1.5f,1.5f);
    }

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    collision.transform.localScale = new Vector2(1, 1);
    //}
}
