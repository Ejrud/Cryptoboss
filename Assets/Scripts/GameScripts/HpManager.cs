using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpManager : MonoBehaviour
{
    [SerializeField] private float hp;
    [SerializeField] private SessionTimer timerManagerScript;

    private Image image;

    private float maxHp;
    private float lastHp;

    // Start is called before the first frame update
    void Start()
    {
        maxHp = 100;
        image = GetComponent<Image>();
        
        hp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        HpChangeMethod();
    }

    private void HpConvertMethod()
    {
        float lengthHpBar = hp / maxHp; // Вычисление длинны полоски здоровья

        //rectTransform.sizeDelta = new Vector2(lengthHpBar, rectTransform.sizeDelta.y); 

        image.fillAmount = lengthHpBar; // Изменение длинны полоски здоровья
    }

    private void HpChangeMethod()
    {
        if(hp != lastHp)
        {
            if((hp <= 100)&&(hp >= 0))
            {
                HpConvertMethod();
            }

            if(hp <= 0)
            {
                GameEndMethod();
            }
            
            lastHp = hp;
        }
    }

    private void GameEndMethod()
    {
        timerManagerScript.isRunning = false; // Остановка таймера
    }

    public void DamageMetod(int damage)
    {
        hp -= damage;
    }
}
