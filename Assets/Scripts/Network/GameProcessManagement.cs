using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameProcessManagement : NetworkBehaviour
{
    [Header("Server functional")]
    [SerializeField] private BattleManager battle;          // Выполняется логика сложения/вычитания карт\

    [SerializeField] private bool debugMode;

    [Header("Server objects")]
    [SerializeField] private Transform allSessions;         // Transform хранящий все сессии
    [SerializeField] private GameObject sessionObject;      // экземпляр сессии

    private List<Session> sessions = new List<Session>();   // Список сессий

    // В Update контролируется вся логика сессий
    private void Update()
    {
        if (isServer)
        {
            // Производится логика конкретной сессии
            foreach (Session currentSession in sessions)
            {
                // Обновление параметров сессии
                currentSession.UpdateSession();

                // Если игроки выбрали карты, то происходит вычисление
                if (currentSession.Ready)
                {
                    battle.SimpleCalculation(currentSession);
                }
            }
        }
    }

    // при запуске сессии (Вызывается из NetworkController) Происходит определение пользователей
    public void PrepareSession(GameObject[] players, string gameMode)
    {
        if (isServer)
        {
            Debug.Log("Create new session");

            Session session = Instantiate(sessionObject, Vector3.zero, Quaternion.identity).GetComponent<Session>();
            session.transform.SetParent(allSessions);

            PlayerNet[] playerNets = new PlayerNet[players.Length];
            for (int i = 0; i < playerNets.Length; i++)
            {
                playerNets[i] = players[i].GetComponent<PlayerNet>();
                players[i].transform.SetParent(session.transform);
            }

            session.Init(playerNets, this, debugMode);
            sessions.Add(session);
        }
    }

    public void RemoveSession(Session session)
    {
        this.sessions.Remove(session);
        Debug.Log("Session removed");
    }
}