using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameProcessManagement : NetworkBehaviour
{
    [Header("Server functional")]
    [SerializeField] private BattleManager battle;

    [Header("Server objects")]
    [SerializeField] private Transform allSessions;
    [SerializeField] private GameObject sessionObject;

    public List<Session> sessions = new List<Session>();

    // Update каждой сессии в списке
    private void Update()
    {
        if (isServer)
        {
            foreach (Session currentSession in sessions)
            {
                currentSession.UpdateSession();

                if (currentSession.Ready)
                {
                    battle.SimpleCalculation(currentSession);
                }
            }
        }
    }

    public void PrepareSession(List<GameObject> players, string gameMode, Session oldSession = null)
    {
        if (isServer)
        {
            if(!oldSession)
            {
                // Debug.Log("Create new session");

                Session session = Instantiate(sessionObject, Vector3.zero, Quaternion.identity).GetComponent<Session>();
                session.transform.SetParent(allSessions);

                PlayerNet[] playerNets = new PlayerNet[players.Count];
                for (int i = 0; i < playerNets.Length; i++)
                {
                    playerNets[i] = players[i].GetComponent<PlayerNet>();
                    players[i].transform.SetParent(session.transform);
                }

                session.Init(playerNets, this, false);
                sessions.Add(session);
            }
            else
            {
                Debug.Log("Recreate session");

                for (int i = 0; i < oldSession.PlayerNets.Length; i++)
                {
                    players[i].transform.SetParent(oldSession.transform);
                }

                oldSession.Init(oldSession.PlayerNets, this, true);
            }
        }
    }

    public void RemoveSession(Session session)
    {
        this.sessions.Remove(session);
        // Debug.Log("Session removed");
    }
}