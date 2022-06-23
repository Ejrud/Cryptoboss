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
    public bool isRunning = false;  // Переклячатель для таймера
    public bool isStoped = false;
    public float RoundTimer;        // Время для раунда
    public float OriginalTime;     // Используется при запуске нового раунда
    private string timerString;     // Текст времени для вывода на UI игрока

    private void Start()
    {
        OriginalTime = RoundTimer;
    }

    // Таймер конкретного раунда (Выполняется только на сервере и отправляется всем клиентам)
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
                isStoped = true;
                session.EndRound();
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
        StartCoroutine(TimerMethod());
    }

    public void ResetTimer()
    {
        // Сброс таймера до изначального значения
        RoundTimer = OriginalTime;
        isRunning = true;
        isStoped = false;
    }

    public string GetTimerText()
    {
        return timerString;
    }
}
