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
            int engDamage = session.PlayerNets[playerQueueIndex].HandCards[cardId].EnergyDamage;
            int Resistance = session.PlayerNets[playerQueueIndex].HandCards[cardId].DamageResistance;
            bool correction = false; // 

            int rivalIndex = 0;

            rivalIndex = playerQueueIndex - 1;
            if (rivalIndex < 0)
            {
                rivalIndex = session.PlayerNets.Length - 1;
            }

            int otherCapital = session.PlayerNets[rivalIndex].Capital;
            float otherEnergy = session.PlayerNets[rivalIndex].Morale;

            // ���. ���������
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
            
            if (session.PlayerNets[rivalIndex].HedgeFundCount <= 0)
            {
                PlayerNet currentPlayer = session.PlayerNets[playerQueueIndex];
                PlayerNet otherPlayer = session.PlayerNets[rivalIndex];

                int playerCount = (session.GameMode == "two") ? 2 : 1;

                switch (session.PlayerNets[playerQueueIndex].HandCards[cardId].Name)
                {
                    case "Turn around": // перенаправляет отрицательные эффекты последней карты противника на его же фишку
                        
                        if (otherPlayer.PlayerImpact.JokerName == "Scam")
                        {
                            for (int i = 0; i < playerCount; i++)
                            {
                                PlayerNet Player = (i != 1) ? currentPlayer : currentPlayer.Friend;
                                PlayerNet Rival = (i != 1) ? otherPlayer : otherPlayer.Friend;

                                CardData[] selfCards = Player.HandCards;
                                CardData[] otherCards = Rival.HandCards;

                                Player.PlayerImpact.JokerName = "Scam"; 

                                for (int j = 0; j < 5; j++)
                                {
                                    selfCards[i].CapitalDamage *= 2;
                                    selfCards[i].CapitalEarnings *= 2;
                                    selfCards[i].EnergyHealth *= 2;

                                    otherCards[j].CapitalDamage /= 2;
                                    otherCards[j].CapitalEarnings /= 2;
                                    otherCards[j].EnergyHealth /= 2;
                                }

                                Player.HandCards = selfCards;
                                Player.UpdateUICards(selfCards);

                                Rival.HandCards = otherCards;
                                Rival.UpdateUICards(otherCards);
                            }
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Liquidation") ///////
                        {
                            currentPlayer.PlayerImpact.JokerName = "Liquidation";
                            if (session.GameMode == "two") currentPlayer.Friend.PlayerImpact.JokerName = "Liquidation";

                            ////////////
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Audit") ///////
                        {
                            currentPlayer.SetAudit(otherPlayer.HandCards);
                            otherPlayer.ResetAudit();
                            currentPlayer.PlayerImpact.JokerName = "Audit";
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "To the moon")
                        {
                            for (int i = 0; i < playerCount; i++)
                            {
                                PlayerNet Player = (i != 1) ? currentPlayer : currentPlayer.Friend;
                                PlayerNet Rival = (i != 1) ? otherPlayer : otherPlayer.Friend;

                                Player.PlayerImpact.JokerName = "To the moon";
                                Player.ToTheMoon = false;
                                Rival.ToTheMoon = true;
                            }
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Pump")
                        {
                            for (int i = 0; i < playerCount; i++)
                            {   
                                PlayerNet Player = (i != 1) ? currentPlayer : currentPlayer.Friend;
                                PlayerNet Rival = (i != 1) ? otherPlayer : otherPlayer.Friend;

                                Player.PlayerImpact.JokerName = "Pump";
                                Player.Pump = true;
                                Rival.Pump = false;
                            }
                        }
                        else
                        {
                            currentPlayer.PlayerImpact.JokerName = "Turn around";

                            queueCapital += otherPlayer.PlayerImpact.CapitalDamage;
                            otherCapital -= otherPlayer.PlayerImpact.CapitalDamage;
                        
                            currentPlayer.PlayerImpact.CapitalDamage = otherPlayer.PlayerImpact.CapitalDamage;
                            currentPlayer.PlayerImpact.CapitalHealth = otherPlayer.PlayerImpact.CapitalDamage;
                        }

                        break;

                    case "Liquidation": // блокирует последнюю карту противника

                        if (otherPlayer.PlayerImpact.JokerName == "Scam")
                        {
                            for (int i = 0; i < playerCount; i++)
                            {
                                PlayerNet Player = (i != 1) ? currentPlayer : currentPlayer.Friend;
                                PlayerNet Rival = (i != 1) ? otherPlayer : otherPlayer.Friend;

                                CardData[] selfCards = Player.HandCards;

                                for (int j = 0; j < 5; j++)
                                {
                                    selfCards[j].CapitalDamage *= 2;
                                    selfCards[j].CapitalEarnings *= 2;
                                    selfCards[j].EnergyHealth *= 2;
                                }

                                Player.HandCards = selfCards;
                                Player.UpdateUICards(selfCards);
                                Player.PlayerImpact.JokerName = "Liquidation";
                            }
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Audit")
                        {
                            for (int i = 0; i < playerCount; i++)
                            {
                                PlayerNet Player = (i != 1) ? currentPlayer : currentPlayer.Friend;
                                PlayerNet Rival = (i != 1) ? otherPlayer : otherPlayer.Friend;

                                Rival.ResetAudit();
                                Player.PlayerImpact.JokerName = "Liquidation";
                            }
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "To the moon")
                        {
                            for (int i = 0; i < playerCount; i++)
                            {
                                PlayerNet Player = (i != 1) ? currentPlayer : currentPlayer.Friend;
                                Player.PlayerImpact.JokerName = "Liquidation";
                                Player.ToTheMoon = false;
                            }
                            
                            cardCost /= 2;
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Pump")
                        {
                            for (int i = 0; i < playerCount; i++)
                            {
                                PlayerNet Player = (i != 1) ? currentPlayer : currentPlayer.Friend;
                                PlayerNet Rival = (i != 1) ? otherPlayer : otherPlayer.Friend;

                                Player.PlayerImpact.JokerName = "Liquidation";
                                Rival.Pump = false;
                            }
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Turn around")
                        {
                            queueCapital += otherPlayer.PlayerImpact.CapitalDamage;

                            for (int i = 0; i < playerCount; i++)
                            {
                                PlayerNet Player = (i != 1) ? currentPlayer : currentPlayer.Friend;
                                Player.PlayerImpact.JokerName = "Liquidation";
                            }
                        }
                        else if (otherPlayer.PlayerImpact.JokerName == "Pivot")
                        {
                            otherCapital -= otherPlayer.PlayerImpact.CapitalDamage;
                            otherEnergy -= otherPlayer.PlayerImpact.Energy;
                        }
                        else
                        {
                            queueCapital += otherPlayer.PlayerImpact.CapitalDamage;
                            currentPlayer.PlayerImpact.CapitalHealth = otherPlayer.PlayerImpact.CapitalDamage; 
                        }
                       break;

                    case "Correction":  // Блокирует опоннента кслать кароту на один ход +
                        correction = true;
                        break;

                    case "Scam":        // В половину урезает характеристики карт соперника +
                        
                        int count = (session.GameMode == "two") ? 2 : 1;

                        for (int i = 0; i < count; i++)
                        {
                            PlayerNet RivalPlayer = (i != 1) ? otherPlayer : otherPlayer.Friend;

                            CardData[] otherCards = RivalPlayer.HandCards;

                            for (int j = 0; j < 5; j++)
                            {
                                otherCards[j].CapitalDamage /= 2;
                                otherCards[j].CapitalEarnings /= 2;
                                otherCards[j].EnergyHealth /= 2;
                            }
                            RivalPlayer.HandCards = otherCards;
                            RivalPlayer.UpdateUICards(otherCards);
                        }

                        currentPlayer.PlayerImpact.JokerName = "Scam";

                        break;

                    case "Hedge fund":  // В течении 3 раундов * на кол-во человек блокирует след карту джокер
                        session.PlayerNets[playerQueueIndex].HedgeFundCount = 3;

                        if (session.GameMode == "two") session.PlayerNets[playerQueueIndex].Friend.HedgeFundCount = 3;

                        queueCapital += capHealth;
                        queueEnergy += engHealth;
                        otherCapital -= capAttack;
                        break;

                    case "Audit":       // Просмотр карт соперника
                        currentPlayer.SetAudit(otherPlayer.HandCards);
                        break;

                    case "To the moon": // Увеличивает стоимость карты противника в 2 раза
                        session.PlayerNets[playerQueueIndex].PlayerImpact.JokerName = "To The Moon";
                        session.PlayerNets[rivalIndex].ToTheMoon = true;

                        if (session.GameMode == "two") session.PlayerNets[rivalIndex].Friend.ToTheMoon = true;

                        break;

                    case "Pump":        // Увеличивает характеристики след. карты в два раза 
                        currentPlayer.PlayerImpact.JokerName = "Pump";
                        currentPlayer.Pump = true;

                        if (session.GameMode == "two")
                        {
                            currentPlayer.Friend.PlayerImpact.JokerName = "Pump";
                            currentPlayer.Friend.Pump = true;
                        } 
                        break;
                    
                    case "Pivot":
                        if (capAttack != 0) // обмен 100 жизней на 2 энергии или наоборот
                        {
                            queueCapital -= capAttack; 
                            queueEnergy += capHealth; // cap health = profit
                            cardCost = 0;

                            currentPlayer.PlayerImpact.CapitalDamage = -capAttack;
                            currentPlayer.PlayerImpact.Energy = capHealth;
                        }
                        else if (capAttack == 0)
                        {
                            queueCapital += capHealth;
                            queueEnergy -= engDamage;
                            cardCost = 0;
                            
                            currentPlayer.PlayerImpact.CapitalDamage = capAttack;
                            currentPlayer.PlayerImpact.Energy = -engDamage;
                        }

                        break;

                    default:
                        currentPlayer.PlayerImpact.JokerName = "";
                        currentPlayer.PlayerImpact.CapitalDamage = capAttack;
                        currentPlayer.PlayerImpact.CapitalHealth = capHealth;
                        queueCapital += capHealth;
                        queueEnergy += engHealth;
                        otherCapital -= capAttack;
                        otherEnergy -= engDamage;
                        break;
                }
            }
            else
            {
                queueCapital += capHealth;
                queueEnergy += engHealth;
                otherCapital -= capAttack;
                otherEnergy -= engDamage;
            }
            
            queueEnergy -= cardCost;
            
            queueCapital = CutSurplusValue(queueCapital, session.PlayerNets[playerQueueIndex].MaxHealth);
            queueEnergy = CutSurplusValueFloat(queueEnergy, 20); // 

            otherCapital = CutSurplusValue(otherCapital, session.PlayerNets[rivalIndex].MaxHealth);
            otherEnergy = CutSurplusValueFloat(otherEnergy, 20);

            if (queueCapital > 0 && otherCapital <= 0)
            {
                Debug.Log($"Player {playerQueueIndex + 1} Win");
                session.PlayerNets[playerQueueIndex].Win = true;
                session.PlayerNets[rivalIndex].Win = false;

                if (session.GameMode == "two")
                {
                    session.PlayerNets[playerQueueIndex].Friend.Win = true;
                    session.PlayerNets[rivalIndex].Friend.Win = false;
                }
            
                session.PlayerNets[playerQueueIndex].UpdatePlayerCharacteristic(queueCapital, queueEnergy, otherCapital, otherEnergy, session.PlayerNets[playerQueueIndex].MaxEnergy);
                session.PlayerNets[rivalIndex].UpdatePlayerCharacteristic(otherCapital, otherEnergy, queueCapital, queueEnergy, session.PlayerNets[rivalIndex].MaxEnergy);
                session.FinishTheGame(session.PlayerNets[0].Win, session.PlayerNets[1].Win);
            }
            else if (queueCapital <= 0 && otherCapital > 0)
            {
                Debug.Log($"Player {rivalIndex + 1} Win");
                session.PlayerNets[rivalIndex].Win = true;
                session.PlayerNets[playerQueueIndex].Win = false;

                if (session.GameMode == "two")
                {
                    session.PlayerNets[rivalIndex].Friend.Win = true;
                    session.PlayerNets[playerQueueIndex].Friend.Win = false;
                }
                    
                session.PlayerNets[playerQueueIndex].UpdatePlayerCharacteristic(queueCapital, queueEnergy, otherCapital, otherEnergy, session.PlayerNets[playerQueueIndex].MaxEnergy);
                session.PlayerNets[rivalIndex].UpdatePlayerCharacteristic(otherCapital, otherEnergy, queueCapital, queueEnergy, session.PlayerNets[rivalIndex].MaxEnergy);
                session.FinishTheGame(session.PlayerNets[0].Win, session.PlayerNets[1].Win);
            }
            else if (queueCapital <= 0 && otherCapital <= 0)
            {
                Debug.Log("Draw");

                session.PlayerNets[playerQueueIndex].UpdatePlayerCharacteristic(queueCapital, queueEnergy, otherCapital, otherEnergy, session.PlayerNets[playerQueueIndex].MaxEnergy);
                session.PlayerNets[rivalIndex].UpdatePlayerCharacteristic(otherCapital, otherEnergy, queueCapital, queueEnergy, session.PlayerNets[rivalIndex].MaxEnergy);
                session.FinishTheGame(false, false);
            }
            else
            {   
                if (session.PlayerNets[playerQueueIndex].HedgeFundCount > 0) session.PlayerNets[playerQueueIndex].HedgeFundCount--;
                
                session.PlayerNets[playerQueueIndex].UpdatePlayerCharacteristic(queueCapital, queueEnergy, otherCapital, otherEnergy, session.PlayerNets[playerQueueIndex].MaxEnergy);
                session.PlayerNets[rivalIndex].UpdatePlayerCharacteristic(otherCapital, otherEnergy, queueCapital, queueEnergy, session.PlayerNets[rivalIndex].MaxEnergy);
                session.PlayerNets[playerQueueIndex].PreviousCard = session.PlayerNets[playerQueueIndex].HandCards[cardId];
                session.PrepareNextRound(correction);
            }
            
            session.SavePlayers();
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
