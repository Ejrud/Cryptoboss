using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAssignmentDamage : MonoBehaviour
{
    public bool isAnimate;

    [Header("Player chips")]
    [SerializeField] private ChipAcceptDamage[] players; // 0 - player 1, 1 - player 2

    [Header("Animation cards")]
    [SerializeField] private CardParameters[] cards; // 0 - player 1, 1 - player 2
    [SerializeField] private Animator animator;


    public void PlayerSetDamage()
    {
        players[0].SetDamage(cards[1].Card.CapitalDamage);
    }

    public void RivalSetDamage()
    {
        players[1].SetDamage(cards[0].Card.CapitalDamage);
    }

    public void AnimateAttack()
    {
        animator.SetTrigger("Play");
        isAnimate = true;
    }

    public void AnimationStoped()
    {
        animator.SetTrigger("Stop");
        isAnimate = false;
    }

    public void UpdateCardParameters(CardData[] cards) // 0 - player, 1 - rival
    {
        for (int i = 0; i < cards.Length; i++)
        {
            this.cards[i].SetCardEffects(cards[i], 1);
        }
    }
}
