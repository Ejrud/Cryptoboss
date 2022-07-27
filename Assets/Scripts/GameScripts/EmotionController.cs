using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmotionController : MonoBehaviour
{
    [SerializeField] private PlayerNet _player;

    [Header("UI")]
    [SerializeField] private Image _playerEmotion;
    [SerializeField] private Image _rivalEmotion;
    [SerializeField] private GameObject _emojiWindow;

    [Header("Emotions")]
    [SerializeField] private Sprite[] _emotions;

    private Animator _animator;
    private bool _animated;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendEmotion(0);
        }
    }

    public void SendEmotion(int index)
    {
        _playerEmotion.sprite = _emotions[index];
        AnimatePlayerEmotion();
        _animated = true;
        _player.CmdSendEmotion(index);
        
    }

    public void RecieveEmotion(int index, string gameMode) // player - игрок который отправил анимацию
    {
        if (gameMode == "one" && !_animated)
        {
            _rivalEmotion.sprite = _emotions[index];
            AnimateRivalEmotion();
        }

        _animated = false;
    }

    public void OpenCloseEmoji()
    {
        _emojiWindow.SetActive(!_emojiWindow.activeInHierarchy);
    }

    private void AnimatePlayerEmotion() // Анимация эмоции игрока
    {
        Debug.Log("Animate player emotion");
        _animator.SetTrigger("PlayerEmoji");
    }
    private void AnimateRivalEmotion() // Анимация эмоции соперника. Сервер всегда выполняет этот метод для отправки эмоций
    {
        Debug.Log("Animate rival emotion");
        _animator.SetTrigger("RivalEmoji");
    }
}
