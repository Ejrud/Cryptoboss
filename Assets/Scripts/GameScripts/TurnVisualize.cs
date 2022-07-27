using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnVisualize : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Text textTurn;
    [SerializeField] private string text = "Your turn";
    [SerializeField] private AnimationCurve turnOpacity;

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

        float apogee = 2f;
        float timer = 0;

        while (timer < apogee)
        {
            textTurn.color = new Vector4(textTurn.color.r, textTurn.color.g, textTurn.color.b, turnOpacity.Evaluate(timer / apogee));
            timer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        textTurn.gameObject.SetActive(false);
        yield return null;
    }
}
