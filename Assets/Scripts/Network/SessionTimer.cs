using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class SessionTimer : NetworkBehaviour
{
    [Header("Session")]
    [SerializeField] private Session session;

    [Header("Timer parameters")]
    public bool isRunning = false;  // ������������� ��� �������
    public bool isStoped = false;
    public float RoundTimer;        // ����� ��� ������
    public float OriginalTime;     // ������������ ��� ������� ������ ������
    private string timerString;     // ����� ������� ��� ������ �� UI ������

    private void Start()
    {
        OriginalTime = RoundTimer;
    }

    // ������ ����������� ������ (����������� ������ �� ������� � ������������ ���� ��������)
    private IEnumerator TimerMethod()
    {
        isRunning = true;

        while (isRunning)
        {
            if (RoundTimer > 0 && !isStoped)
            {
                RoundTimer -= 1f;
            }
            if (RoundTimer <= 0 && !isStoped)
            {
                if(!session.AwaitPlayer)
                {
                    isStoped = true;
                    session.EndRound();
                }
                else
                {
                    bool[] playerWins = new bool[session.PlayerNets.Length];

                    for (int i = 0; i < session.PlayerNets.Length; i++)
                    {
                        if (session.PlayerNets[i] != null)
                        {
                            playerWins[i] = true;
                        }
                        else
                        {
                            playerWins[i] = false;
                        }
                    }

                    session.FinishTheGame(playerWins[0], playerWins[1]);
                }
            }

            int min = (int)RoundTimer / 60;
            int sec = (int)RoundTimer % 60;
            timerString = string.Format("{0:0}:{1:00}", min, sec);

            session.UpdatePlayerTimer(timerString);

            yield return new WaitForSeconds(1f);
            
        }

        yield return null;
    }

    public void StartTimer()
    {
        Debug.Log("Timer started");
        StopAllCoroutines();
        StartCoroutine(TimerMethod());
    }

    public void ResetTimer()
    {
        // ����� ������� �� ������������ ��������
        RoundTimer = OriginalTime;
        isRunning = true;
        isStoped = false;
    }

    public string GetTimerText()
    {
        return timerString;
    }
}
