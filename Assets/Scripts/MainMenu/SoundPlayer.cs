using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSrc;
    [SerializeField] private Slider _musicValume;
    [SerializeField] private User _user;

    [Header("UI")]
    [SerializeField] private Image _muteImage;
    [SerializeField] private Sprite[] _activeButtons; // 0 - active / 1 - inactive

    private float volume;

    private void Start()
    {
        OnMute();
        _audioSrc.volume = _musicValume.value;
        PlayerPrefs.SetFloat("musicVolume", _audioSrc.volume);
    }

    public void PlayMusic()
    {
       _audioSrc.Play();
    }

    public void OnChangeVolume()
    {
        _audioSrc.volume = _musicValume.value;
        PlayerPrefs.SetFloat("musicVolume", _audioSrc.volume);
    }

    public void OnChangeMute()
    {
        _user.Mute = !_user.Mute;
        _audioSrc.mute = (_user.Mute) ? true : false;
        OnMute();
    }

    private void OnMute()
    {
        _audioSrc.mute = (_user.Mute) ? true : false;
        _muteImage.sprite = (_user.Mute) ? _activeButtons[1] : _activeButtons[0];
    }
}
