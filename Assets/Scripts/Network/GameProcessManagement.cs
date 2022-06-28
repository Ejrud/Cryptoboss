using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameProcessManagement : NetworkBehaviour
{
    [Header("Server functional")]
    [SerializeField] private BattleManager battle;          // ����������� ������ ��������/��������� ����\

    [SerializeField] private bool debugMode;

    [Header("Server objects")]
    [SerializeField] private Transform allSessions;         // Transform �������� ��� ������
    [SerializeField] private GameObject sessionObject;      // ��������� ������

    private List<Session> sessions = new List<Session>();   // ������ ������

    // � Update �������������� ��� ������ ������
    private void Update()
    {
        if (isServer)
        {
            // ������������ ������ ���������� ������
            foreach (Session currentSession in sessions)
            {
                // ���������� ���������� ������
                currentSession.UpdateSession();

                // ���� ������ ������� �����, �� ���������� ����������
                if (currentSession.Ready)
                {
                    battle.SimpleCalculation(currentSession);
                }
            }
        }
    }

    // ��� ������� ������ (���������� �� NetworkController) ���������� ����������� �������������
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