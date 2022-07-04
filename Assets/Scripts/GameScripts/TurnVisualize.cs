using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnVisualize : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Text textTurn;
    [SerializeField] private string text = "Your turn";

    private Color textDefaultColor;

    private void Start()
    {
        textDefaultColor = textTurn.color;
    }

    public void SetTurn()
    {
        StartCoroutine(SetAlert(text));
    }

    private IEnumerator SetAlert(string text)
    {
        textTurn.gameObject.SetActive(true);
        textTurn.text = text;

        float apogee = 4f;
        float timer = apogee;

        while (timer > 0)
        {
            textTurn.color = new Vector4(textTurn.color.r, textTurn.color.g, textTurn.color.b, timer/apogee);
            timer -= Time.deltaTime;
            yield return new WaitForUpdate();
        }

        textTurn.gameObject.SetActive(false);
        yield return null;
    }
}
