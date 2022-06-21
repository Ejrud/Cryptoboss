using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mirror;
// using Org.BouncyCastle.Asn1.Esf;
// using Unity.PlasticSCM.Editor.WebApi;

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

            // поиск индекса соперника
            int rivalIndex = 0;

            for (int i = 0; i < session.PlayerNets.Length; i++)
            {
                if (i != session.PlayerIndexQueue)
                {
                    rivalIndex = i;
                }
            }

            // Капитал и энегрия другого игрока
            int otherCapital = session.PlayerNets[rivalIndex].Capital;
            float otherEnergy = session.PlayerNets[rivalIndex].Morale;

            // доп. параметры
            if (session.PlayerNets[playerQueueIndex].ToTheMoon)
            {
                cardCost *= 2;
                session.PlayerNets[playerQueueIndex].ToTheMoon = false;
            }
            else if (session.PlayerNets[playerQueueIndex].Pump)
            {
                capAttack *= 2;
                capHealth *= 2;
                engHealth *= 2;
                
                session.PlayerNets[playerQueueIndex].Pump = false;
            }

            #endregion
            
            // Если у игрока не была выбрана карта HedgeFund, то выпол
            if (session.PlayerNets[rivalIndex].HedgeFundCount <= 0)
            {
                PlayerNet currentPlayer = session.PlayerNets[playerQueueIndex];
                PlayerNet otherPlayer = session.PlayerNets[rivalIndex];
                
                switch (session.PlayerNets[playerQueueIndex].HandCards[cardId].Name)
                {
                    case "Turn around": // Отрицательные эффекты прошлой карты накладываются на самого игрока (у текущего востанавливаются до изначального значения)
                        
                        if (otherPlayer.PlayerImpact.JokerName == "Scam")
                        {
                            currentPlayer.PlayerImpact.JokerName = "Scam";
                            CardData[] selfCards = currentPlayer.HandCards;
                            CardData[] rivalCards = otherPlayer.HandCards;
                            
                            for (int i = 0; i < 5; i++)
                            {
                                selfCards[i].CapitalDamage *= 2;
                                selfCards[i].CapitalEarnings *= 2;
                                selfCards[i].EnergyHealth *= 2;
                                
                                rivalCards[i].CapitalDamage /= 2;
                                rivalCards[i].CapitalEarnings /= 2;
                                rivalCards[i].EnergyHealth /= 2;
                            }
                            currentPlayer.HandCards = selfCards;
                            currentPlayer.UpdateUICards(selfCards);
                            
                            otherPlayer.HandCards = rivalCards;
                            otherPlayer.UpdateUICards(rivalCards);
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Liquidation") ///////
                        {
                            currentPlayer.PlayerImpact.JokerName = "Liquidation";
                            
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Audit") ///////
                        {
                            // Отображать карты соперника, а свои спрятать
                            currentPlayer.SetAudit(otherPlayer.HandCards);
                            otherPlayer.ResetAudit();
                            currentPlayer.PlayerImpact.JokerName = "Audit";
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "To the moon")
                        {
                            currentPlayer.PlayerImpact.JokerName = "To the moon";
                            currentPlayer.ToTheMoon = false;
                            otherPlayer.ToTheMoon = true;
                            
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Pump")
                        {
                            currentPlayer.PlayerImpact.JokerName = "Pump";
                            currentPlayer.Pump = true;
                            otherPlayer.Pump = false;
                        }
                        else
                        {
                            currentPlayer.PlayerImpact.JokerName = "Turn around";

                            queueCapital += otherPlayer.PlayerImpact.CapitalDamage;
                            otherCapital -= otherPlayer.PlayerImpact.CapitalDamage;
                        
                            // Сохранение действий конкретного игрока
                            currentPlayer.PlayerImpact.CapitalDamage = otherPlayer.PlayerImpact.CapitalDamage;
                            currentPlayer.PlayerImpact.CapitalHealth = otherPlayer.PlayerImpact.CapitalDamage;
                        }

                        break;

                    case "Liquidation": // Нейтрализация последней карты соперника
                        currentPlayer.PlayerImpact.JokerName = "Liquidation";
                        
                        if (otherPlayer.PlayerImpact.JokerName == "Scam")
                        {
                            CardData[] selfCards = currentPlayer.HandCards;
                            
                            for (int i = 0; i < 5; i++)
                            {
                                selfCards[i].CapitalDamage *= 2;
                                selfCards[i].CapitalEarnings *= 2;
                                selfCards[i].EnergyHealth *= 2;
                            }
                            
                            currentPlayer.HandCards = selfCards;
                            currentPlayer.UpdateUICards(selfCards);
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Audit") ///////
                        {
                            otherPlayer.ResetAudit();
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "To the moon")
                        {
                            currentPlayer.ToTheMoon = false;
                            cardCost /= 2;
                            
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Pump")
                        {
                            otherPlayer.Pump = false;
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Turn around")
                        {
                            queueCapital += otherPlayer.PlayerImpact.CapitalDamage;
                        }
                        else
                        {
                            queueCapital += otherPlayer.PlayerImpact.CapitalDamage;
                        
                            // Сохранение действий конкретного игрока
                            currentPlayer.PlayerImpact.CapitalHealth = otherPlayer.PlayerImpact.CapitalDamage; 
                        }
                       break;

                    case "Correction":  // Блокирует оппонента класть 1 карту на стол
                        correction = true;
                        break;

                    case "Scam":        // В половину урезает характеристики карт соперника ////////
                        CardData[] otherCards = otherPlayer.HandCards;
                        for (int i = 0; i < 5; i++)
                        {
                            otherCards[i].CapitalDamage /= 2;
                            otherCards[i].CapitalEarnings /= 2;
                            otherCards[i].EnergyHealth /= 2;
                        }
                        otherPlayer.HandCards = otherCards;
                        otherPlayer.UpdateUICards(otherCards);
                        currentPlayer.PlayerImpact.JokerName = "Scam";
                        break;

                    case "Hedge fund":  // Блок от джокера в течении 3 ходов
                        session.PlayerNets[playerQueueIndex].HedgeFundCount = 3;
                        queueCapital += capHealth;
                        queueEnergy += engHealth;
                        otherCapital -= capAttack;
                        break;

                    case "Audit":       // Видимость характеристик карт соперника
                        currentPlayer.SetAudit(otherPlayer.HandCards);
                        break;

                    case "To the moon": // След противника карта потребует в 2 раза больше энергии
                        session.PlayerNets[playerQueueIndex].PlayerImpact.JokerName = "To The Moon";
                        session.PlayerNets[rivalIndex].ToTheMoon = true;
                        break;

                    case "Pump":        // Увеличивает след. карту игрока в 2 раза
                        currentPlayer.PlayerImpact.JokerName = "Pump";
                        currentPlayer.Pump = true;
                        break;

                    default:
                        currentPlayer.PlayerImpact.JokerName = "";
                        queueCapital += capHealth;
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
            
            queueEnergy -= cardCost;
            
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
                if (session.PlayerNets[playerQueueIndex].HedgeFundCount > 0) session.PlayerNets[playerQueueIndex].HedgeFundCount--;
                
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
