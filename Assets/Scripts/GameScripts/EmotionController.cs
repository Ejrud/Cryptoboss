using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmotionController : MonoBehaviour
{
    [SerializeField] private PlayerNet _player;

    [Header("UI")]
    [SerializeField] private Image _playerEmotion;
    [SerializeField] private Image[] _playerEffects;
    [SerializeField] private Image _rivalEmotion;
    [SerializeField] private Image[] _rivalEffects;
    [SerializeField] private GameObject _emojiWindow;

    [Header("Emotions")]

    [SerializeField] private List<Emoji> emoji;

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
        _playerEmotion.sprite = emoji[index].Emotion;

        for (int i = 0; i < _playerEffects.Length; i++)
        {
            _playerEffects[i].sprite = emoji[index].Effect;
        }

        AnimatePlayerEmotion();
        _animated = true;
        _player.CmdSendEmotion(index);
        
    }

    public void RecieveEmotion(int index, string gameMode) // player - игрок который отправил анимацию
    {
        if (gameMode == "one" && !_animated)
        {
            _rivalEmotion.sprite = emoji[index].Emotion;

        for (int i = 0; i < _rivalEffects.Length; i++)
        {
            _rivalEffects[i].sprite = emoji[index].Effect;
        }
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

    [System.Serializable]
    public class Emoji
    {
        public string Name;
        public Sprite Emotion; // main
        public Sprite Effect;
    }
}
