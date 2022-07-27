using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EmotionNet: NetworkBehaviour
{
    [Header("Emotions settings")]
    [SerializeField] private Session _session;
    [SerializeField] private float _cooldown = 3;

    private bool _playEmoji = false;

    public void PlayEmotion(int index)
    {
        if (!_playEmoji)
        {
            _playEmoji = true;
            StartCoroutine(Cooldown());
            foreach (PlayerNet playNet in _session.PlayerNets)
            {
                playNet.RpcRecieveEmotion(index, _session.GameMode);
            }
        }
    }

    private IEnumerator Cooldown() // Задержка
    {
        float timer = _cooldown;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        _playEmoji = false;

        yield return null;
    }
}
