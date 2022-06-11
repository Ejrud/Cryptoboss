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
            int queueEnergy = session.PlayerNets[playerQueueIndex].Morale;

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
            int otherEnergy = session.PlayerNets[rivalIndex].Morale;

            #endregion

            // 
            switch (session.PlayerNets[playerQueueIndex].HandCards[cardId].Type)
            {
                case "turn around":
                    if (session.Turn_around)
                    {
                        // 
                        queueCapital += capHealth;
                        queueEnergy -= cardCost;
                        queueEnergy += engHealth;
                        queueCapital -= capAttack;
                        session.Turn_around = false;
                    }
                    else
                    {
                        // 
                        session.Turn_around = true;
                    }
                break;

                case "liquidation": // Отмена карты соперника
                    if (session.PlayerNets[rivalIndex].PreviousCard == null) break;
                    queueCapital += session.PlayerNets[rivalIndex].PreviousCard.CapitalDamage;
                    // 
                break;

                case "correction":
                    correction = true;
                break;

                case "scam": // 
                    CardData[] cardVar = session.PlayerNets[rivalIndex].HandCards;
                    for (int i = 0; i < 5; i++)
                    {
                        cardVar[i].CapitalDamage /= 2;
                        cardVar[i].CapitalEarnings /= 2;
                        cardVar[i].EnergyHealth /= 2;
                    }
                    session.PlayerNets[rivalIndex].HandCards = cardVar;
                    session.PlayerNets[rivalIndex].UpdateUICards(cardVar);
                break;

                case "hedge fund":
                    // 
                    session.PlayerNets[playerQueueIndex].HedgeFundCount = 3;
                    queueCapital += capHealth;
                    queueEnergy -= cardCost;
                    queueEnergy += engHealth;
                    otherCapital -= capAttack;
                break;

                case "audit":

                break;

                case "to the moon":

                break;

                case "pump":

                break;

                default:
                    queueCapital += capHealth;
                    queueEnergy -= cardCost;
                    queueEnergy += engHealth;
                    otherCapital -= capAttack;
                break;
            }

            queueCapital = CutSurplusValue(queueCapital, session.PlayerNets[playerQueueIndex].MaxHealth);
            queueEnergy = CutSurplusValue(queueEnergy, 20); // 

            if (queueCapital > 0 && otherCapital <= 0)
            {
                Debug.Log($"Player {playerQueueIndex + 1} Win");
                SetRevard(session.PlayerNets[playerQueueIndex].Wallet, "10");
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
                SetRevard(session.PlayerNets[rivalIndex].Wallet, "10");
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

    private async void SetRevard(string wallet, string amount)
    {
        string uri = "https://cryptoboss.win/ajax/models/messages/customizers/mint_token1_xy8q554qo?address=" + wallet + "&amount=" + amount;
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        await webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Player rewaded");
        }
        else
        {
            Debug.Log(webRequest.error);
        }

    }

    private void TurnAround()
    {
        
    }
}
