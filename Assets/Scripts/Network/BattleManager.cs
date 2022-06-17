using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mirror;

public class BattleManager : NetworkBehaviour
{   
    private TypeDetermine typeDetermine;
    private void Start()
    {
        typeDetermine = GetComponent<TypeDetermine>();
    }

    public void SimpleCalculation(Session session)
    {
        if (isServer)
        {
            session.Ready = false;
            int playerQueueIndex = session.PlayerIndexQueue;

            #region Players stats

            // 
            int queueCapital = session.PlayerNets[playerQueueIndex].Capital;
            float queueEnergy = session.PlayerNets[playerQueueIndex].Morale;

            // 
            int cardId = session.PlayerNets[playerQueueIndex].SelectedCardId;
            int capAttack = session.PlayerNets[playerQueueIndex].HandCards[cardId].CapitalDamage;
            int cardCost = session.PlayerNets[playerQueueIndex].HandCards[cardId].CardCost;
            int capHealth = session.PlayerNets[playerQueueIndex].HandCards[cardId].CapitalEarnings;
            int engHealth = session.PlayerNets[playerQueueIndex].HandCards[cardId].EnergyHealth;
            int Resistance = session.PlayerNets[playerQueueIndex].HandCards[cardId].DamageResistance;
            bool correction = false; // 

            // 
            int rivalIndex = 0;

            for (int i = 0; i < session.PlayerNets.Length; i++)
            {
                if (i != session.PlayerIndexQueue)
                {
                    rivalIndex = i;
                }
            }

            // 
            int otherCapital = session.PlayerNets[rivalIndex].Capital;
            float otherEnergy = session.PlayerNets[rivalIndex].Morale;

            for (int i = 0; i < session.PlayerNets.Length; i++)
            {
                if (session.PlayerNets[i].HedgeFundCount > 0)
                {
                    session.PlayerNets[i].HedgeFundCount--;
                }
            }

            if (session.ToTheMoon)
            {
                cardCost *= 2;
            }

            #endregion
            
            // ���� � ������ �� ���� ������� ����� HedgeFund, �� �����
            if (session.PlayerNets[rivalIndex].HedgeFundCount < 0)
            {
                switch (session.PlayerNets[playerQueueIndex].HandCards[cardId].Name)
                {
                    case "Turn around": // ������������� ������� ������� ����� ������������� �� ������ ������ (� �������� ���������������� �� ������������ ��������)
                        PlayerNet current = session.PlayerNets[playerQueueIndex];
                        PlayerNet other = session.PlayerNets[rivalIndex];
                        
                        queueCapital += other.PlayerImpact.CapitalDamage;
                        otherCapital -= other.PlayerImpact.CapitalDamage;

                        current.PlayerImpact.JokerName = "Turn around";
                        current.PlayerImpact.CapitalDamage = other.PlayerImpact.CapitalDamage;
                        break;

                    case "Liquidation": // ������������� ��������� ����� ���������
                        queueCapital += session.PlayerNets[rivalIndex].PlayerImpact.CapitalDamage;
                        session.PlayerNets[playerQueueIndex].PlayerImpact.CapitalHealth = session.PlayerNets[rivalIndex].PlayerImpact.CapitalDamage;
                        break;

                    case "Correction": // ��������� ��������� ������ ����� �� ����
                        correction = true;
                        break;

                    case "Scam": // � �������� ������� ��������������
                        CardData[] otherCards = session.PlayerNets[rivalIndex].HandCards;
                        for (int i = 0; i < 5; i++)
                        {
                            otherCards[i].CapitalDamage /= 2;
                            otherCards[i].CapitalEarnings /= 2;
                            otherCards[i].EnergyHealth /= 2;
                        }
                        session.PlayerNets[rivalIndex].HandCards = otherCards;
                        session.PlayerNets[rivalIndex].UpdateUICards(otherCards);
                        session.PlayerNets[playerQueueIndex].PlayerImpact.Joker = true;
                        session.PlayerNets[playerQueueIndex].PlayerImpact.JokerName = "Scam";
                        break;

                    case "Hedge fund": // ���� �� ������� � ������� 3 �����
                        session.PlayerNets[playerQueueIndex].HedgeFundCount = 3;
                        queueCapital += capHealth;
                        queueEnergy -= cardCost;
                        queueEnergy += engHealth;
                        otherCapital -= capAttack;
                        break;

                    case "Audit": // ��������� ������������� ���� ���������
                        
                        break;

                    case "To the moon": // ���� ����� ��������� � 2 ���� ������ �������
                        session.ToTheMoon = true;
                        break;

                    case "Pump": // ����������� �������������� ����� � 2 ����
                        queueCapital += capHealth * 2;
                        queueEnergy -= cardCost * 2;
                        queueEnergy += engHealth * 2;
                        otherCapital -= capAttack * 2;

                        PlayerNet player = session.PlayerNets[playerQueueIndex];

                        player.PlayerImpact.Joker = true;
                        player.PlayerImpact.JokerName = "Pump";
                        player.PlayerImpact.CapitalDamage = capAttack * 2; 
                        break;

                    default:
                        queueCapital += capHealth;
                        queueEnergy -= cardCost;
                        queueEnergy += engHealth;
                        otherCapital -= capAttack;
                        break;
                }
            }
            else
            {
                queueCapital += capHealth;
                queueEnergy -= cardCost;
                queueEnergy += engHealth;
                otherCapital -= capAttack;
            }
            

            queueCapital = CutSurplusValue(queueCapital, session.PlayerNets[playerQueueIndex].MaxHealth);
            queueEnergy = CutSurplusValueFloat(queueEnergy, 20); // 

            if (queueCapital > 0 && otherCapital <= 0)
            {
                Debug.Log($"Player {playerQueueIndex + 1} Win");
                session.PlayerNets[playerQueueIndex].Win = true;

                for (int i = 0; i < session.PlayerNets.Length; i++)
                {
                    if (playerQueueIndex != i)
                    {
                        session.PlayerNets[i].Win = false;
                    }
                }
                session.FinishTheGame(session.PlayerNets[0].Win, session.PlayerNets[1].Win);
            }
            else if (queueCapital <= 0 && otherCapital > 0)
            {
                Debug.Log($"Player {rivalIndex + 1} Win");
                session.PlayerNets[rivalIndex].Win = true;

                for (int i = 0; i < session.PlayerNets.Length; i++)
                {
                    if (rivalIndex != i)
                    {
                        session.PlayerNets[i].Win = false;
                    }
                }
                session.FinishTheGame(session.PlayerNets[0].Win, session.PlayerNets[1].Win);
            }
            else if (queueCapital <= 0 && otherCapital <= 0)
            {
                Debug.Log("Draw");
                session.FinishTheGame(false, false);
            }
            else
            {   
                if (session.PlayerNets[playerQueueIndex].HedgeFundCount >= 0) session.PlayerNets[playerQueueIndex].HedgeFundCount--;
                
                session.PlayerNets[playerQueueIndex].UpdatePlayerCharacteristic(queueCapital, queueEnergy, otherCapital, otherEnergy, queueEnergy);
                session.PlayerNets[rivalIndex].UpdatePlayerCharacteristic(otherCapital, otherEnergy, queueCapital, queueEnergy, otherEnergy);
                session.PlayerNets[playerQueueIndex].PreviousCard = session.PlayerNets[playerQueueIndex].HandCards[cardId];
                session.PrepareNextRound(correction);
            }
        }
    }

    // 
    private int CutSurplusValue(int concreteValue, int maxValue)
    {
        if (concreteValue > maxValue)
        {
            concreteValue = maxValue;
        }
        if(concreteValue < 0)
        {
            concreteValue = 0;
        }

        return concreteValue;
    }

    private float CutSurplusValueFloat(float concreteValue, float maxValue)
    {
        if (concreteValue > maxValue)
        {
            concreteValue = maxValue;
        }
        if(concreteValue < 0)
        {
            concreteValue = 0;
        }

        return concreteValue;
    }
}
