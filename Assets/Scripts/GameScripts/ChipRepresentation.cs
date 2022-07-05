using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChipRepresentation : MonoBehaviour
{
    [SerializeField] private Text[] teamText;
    [SerializeField] private Text[] team_1;
    [SerializeField] private Text[] team_2;
    [SerializeField] private string[] teamName = {"Team 1", "Team 2"};

    public void SetUpWindows(string gameMode = "one") // Если нужно показывать имена игроков, то передать их в этот метод
    {
        if (gameMode == "one")
        {
            teamText[0].gameObject.SetActive(false);
            teamText[1].gameObject.SetActive(false);
            
            team_1[0].text = "Player 1";
            team_2[0].text = "Player 2";
        }
        else if (gameMode == "two")
        {
            teamText[0].gameObject.SetActive(true);
            teamText[1].gameObject.SetActive(true);

            teamText[0].text = teamName[0];
            teamText[1].text = teamName[1];

            for (int i = 0; i < team_1.Length; i++)
            {
                team_1[i].text = $"Player {i + 1}";
                team_2[i].text = $"Player {i + 1}";
            }
        }
        else if (gameMode == "three")
        {
            teamText[0].gameObject.SetActive(false);
            teamText[1].gameObject.SetActive(false);

            team_1[0].text = "Player 1";
            team_2[0].text = "Player 2";
        }
    }
}
