using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadRewardAnimation : MonoBehaviour
{
    [SerializeField] private string[] loadText = {"Load reward.", "Load reward..", "Load reward..."};
    [SerializeField] private TMP_Text LoadingTxt;
    [SerializeField] private float animateRate = 0.5f;

    private void Start()
    {
        StartCoroutine(LoadAnimation());
    }

    private IEnumerator LoadAnimation()
    {
        bool animation = true;
        int counter = 0;
        
        while (animation)
        {
            LoadingTxt.text = loadText[counter];
            counter++;

            if(counter >= loadText.Length)
            {
                counter = 0;
            }

            yield return new WaitForSeconds(animateRate); 
        }

        yield return null;
    }
}
