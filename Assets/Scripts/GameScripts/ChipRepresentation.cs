using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChipRepresentation : MonoBehaviour
{
    [SerializeField] private Text[] chipNumber_1;
    [SerializeField] private Text[] chipNumber_2;
    [SerializeField] private Text[] team_1;
    [SerializeField] private Text[] team_2;
    [SerializeField] private string[] teamName = {"Team 1", "Team 2"};

    public void SetUpWindows(string[] names, string[] chipNames, string gameMode = "one") // Если нужно показывать имена игроков, то передать их в этот метод // 0 - игрок, 1 - соперник, 2 - союзник, 3 - союзник соперника
    {
        if (gameMode == "one")
        {
            team_1[0].gameObject.SetActive(true);
            team_2[0].gameObject.SetActive(true);

            team_1[0].text = names[0];
            team_2[0].text = names[1];

            chipNumber_1[0].text = chipNames[0];
            chipNumber_2[0].text = chipNames[1];
        }
        else if (gameMode == "two")
        {
            team_1[0].text = names[0];
            team_1[1].text = names[2];
            chipNumber_1[0].text = chipNames[0];
            chipNumber_1[1].text = chipNames[2];

            team_2[0].text = names[1];
            team_2[1].text = names[3];
            chipNumber_2[0].text = chipNames[1];
            chipNumber_2[1].text = chipNames[3];
        }
        else if (gameMode == "three")
        {
            team_1[0].text = names[0];
            team_2[0].text = names[1];
        }
    }
}
