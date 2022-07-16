using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChipAcceptDamage : MonoBehaviour
{
    public enum PlayerType { Player, Rival };
    [SerializeField] private PlayerType playerType;
    [SerializeField] private PlayerNet playerNet;
    [SerializeField] private Animation anim;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] prefabTexts = new GameObject[2];
    [SerializeField] private float visibleTime;
    [SerializeField] private float speed = 10;
    [SerializeField] private Transform spawnPosition;

    public void SetDamage(int Capital)
    {
        if (playerType == PlayerType.Player)
        {
            Debug.Log("Capital damage = " + Capital);
            if (Capital != 0)
            {
                anim.Play();
            }
            StartCoroutine(DamageValueText(Capital));
        }
        else
        {
            Debug.Log("Capital damage = " + Capital);
            if (Capital != 0)
            {
                anim.Play();
            }
            StartCoroutine(DamageValueText(Capital));
        }
    }

    private IEnumerator DamageValueText(int dmgs) // 0 - capital/ 1 - morale
    {
        bool animate = true;
        Text[] texts = new Text[2];
        float timer = visibleTime;

        
            texts[0] = Instantiate(prefabTexts[0], spawnPosition.position, Quaternion.identity).GetComponent<Text>();
            texts[0].transform.SetParent(spawnPosition);
            texts[0].transform.localScale = new Vector3(1, 1, 1);
            texts[0].text = "-" + dmgs.ToString();
        
        Debug.Log("Animate damage");

        while (animate)
        {
            timer -= Time.deltaTime;

            
            texts[0].transform.Translate(Vector3.up * Time.deltaTime * speed);
            texts[0].color = new Vector4(texts[0].color.r, texts[0].color.g, texts[0].color.b, texts[0].color.a - Time.deltaTime);
            

            if (timer <= 0 ) animate = false;

            yield return new WaitForFixedUpdate();
        }
        
        Destroy(texts[0].gameObject);

        yield return null;
    }
}
