using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameProcessManagement : NetworkBehaviour
{
    [Header("Server functional")]
    [SerializeField] private BattleManager battle;          // Выполняется логика сложения/вычитания карт

    [Header("Server objects")]
    [SerializeField] private Transform allSessions;         // Transform хранящий все сессии
    [SerializeField] private GameObject sessionObject;      // экземпляр сессии

    [Header("\"chips data base\"")]
    public ChipData[] chipData;
    private List<Session> sessions = new List<Session>();   // Список сессий

    // При запуске брать из бд все фишки с их характеристиками
    private void Start()
    {
        // Здесь сделать загрузку всех фишек из базы данных!!!!
        // ChipData[] chips = new ChipData[allChips]
    }

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
    public void PrepareSession(GameObject[] players)
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

            session.Init(playerNets, this);
            sessions.Add(session);
        }
    }

    public void RemoveSession(Session session)
    {
        this.sessions.Remove(session);
        Debug.Log("Session removed");
    }

    // При наличии базы данных выгржать только из нее!!!!!!!! (Вызывается в Session)
    public ChipData[] GetAllChips()
    {
        return chipData;
    }
}