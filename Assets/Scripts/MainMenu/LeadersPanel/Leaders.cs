using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leaders : MonoBehaviour
{
    [Header("Chips")]
    [SerializeField] private Image[] chipImage = new Image[3];

    [Header("Leaders table")]
    [SerializeField] private Text[] nameRows = new Text[10];
    [SerializeField] private Text[] scoreRows = new Text[10];

    //// Вместо этого использовать данные из бд Altrp! ////
    private string[] randomNames = { "John", "Adam", "Andrew", "Carl", "Clifford", "Dirk", "Earl", "Eric", "Lan" };
    private string[] randomSdNames = { "Williams", "Peters", "Gibson", "Martin", "Jordan", "Jackson", "Grant", "Davis", "Collins" };
    private Color[] randomColors = { Color.black, Color.green, Color.blue, Color.gray, Color.magenta, Color.cyan };
    // 

    // Со стороны сервера на 7 дней записывается весь прогресс пользователей, по окончаню сбрасывается. Так же и с сезоном.

    // Массив имен за неделю (С сервера берется object или json и вместо этих массивов записываются данные с сервера)
    private string[] namesOfWeek = new string[10];
    private int[] scoreOfWeek = new int[10];
    private Color[] chipColorWeek = new Color[10]; // фишка конкретного игрока (узнать какая именно фишка будет видна другими пользователями!)

    // Массив имен за сезон
    private string[] namesOfSeason = new string[10];
    private int[] scoreOfSeason = new int[10];
    ////

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            namesOfWeek[i] = randomNames[Random.Range(0, 9)] + " " + randomSdNames[Random.Range(0, 9)];
            scoreOfWeek[i] = Random.Range(0, 1000);
            chipColorWeek[i] = randomColors[Random.Range(0, randomColors.Length)];
        }

        for (int i = 0; i < 10; i++)
        {
            namesOfSeason[i] = randomNames[Random.Range(0, 9)] + " " + randomSdNames[Random.Range(0, 9)];
            scoreOfSeason[i] = Random.Range(0, 1000);
        }

        BubleSort();
        SetLeadersOfWeek();
    }

    public void SetLeadersOfWeek() // Вызывается кнопками "Сезон" "Неделя"
    {
        UpdateLeaderBoard(namesOfWeek, scoreOfWeek);
    }

    public void SetLeadersOfSeason()
    {
        UpdateLeaderBoard(namesOfSeason, scoreOfSeason);
    }

    private void BubleSort()  // Сортировка по возрастанию
    {
        for (int i = 0; i < scoreOfWeek.Length; i++)
        {
            for (int j = 0; j < scoreOfWeek.Length - 1; j++)
            {
                if (scoreOfWeek[j] < scoreOfWeek[j + 1])
                {
                    int saveScore = scoreOfWeek[j];
                    scoreOfWeek[j] = scoreOfWeek[j + 1];
                    scoreOfWeek[j + 1] = saveScore;

                    string saveName = namesOfWeek[j];
                    namesOfWeek[j] = namesOfWeek[j + 1];
                    namesOfWeek[j + 1] = saveName;
                }
            }
        }

        for (int i = 0; i < scoreOfSeason.Length; i++)
        {
            for (int j = 0; j < scoreOfSeason.Length - 1; j++)
            {
                if (scoreOfSeason[j] < scoreOfSeason[j + 1])
                {
                    int saveScore = scoreOfSeason[j];
                    scoreOfSeason[j] = scoreOfSeason[j + 1];
                    scoreOfSeason[j + 1] = saveScore;

                    string saveName = namesOfSeason[j];
                    namesOfSeason[j] = namesOfSeason[j + 1];
                    namesOfSeason[j + 1] = saveName;
                }
            }
        }
    }

    private void UpdateLeaderBoard(string[] names, int[] scores)
    {
        for (int i = 0; i < names.Length; i++)
        {
            nameRows[i].text = names[i];
            scoreRows[i].text = scores[i].ToString();
        }
    }
}
