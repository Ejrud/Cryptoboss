using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAnimationController : MonoBehaviour
{
    public bool isScaled = false;
    public bool isAnimated = false;
    [SerializeField] private ChipAcceptDamage playerChipAcceptDamage;
    [SerializeField] private ChipAcceptDamage rivalChipAcceptDamage;
    [SerializeField] private AnimationCurve moveCurve; // сделать плавное снижение к фишке
    [SerializeField] private Animation selectAnim;
    [SerializeField] private AnimationClip selectClip;
    [SerializeField] private AnimationClip scaleDown;
    [SerializeField] private AnimationClip scaleUp;
    private int localElapsed;
    private CardData cardData;
    private CardData rivalData;

    public void UpdateStats(CardData card)
    {
        cardData = card;
    }

    public void CardSelect()
    {
        selectAnim.clip = selectClip;
        selectAnim.Play();
    }

    public void ScaleDown()
    {
        selectAnim.clip = scaleDown;
        isScaled = true;
        selectAnim.Play();
    }

    public void ScaleUp()
    {
        selectAnim.clip = scaleUp;
        isScaled = false;
        selectAnim.Play();
    }

    public void AnimationStoped()
    {
        isAnimated = false;
    }
}   
