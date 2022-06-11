using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private Slider musicValume;
    private float volume;

    private void Start()
    {
        audioSrc.volume = musicValume.value;
        PlayerPrefs.SetFloat("musicVolume", audioSrc.volume);
    }

    public void PlayMusic()
    {
       audioSrc.Play();
    }

    public void OnChangeVolume()
    {
        audioSrc.volume = musicValume.value;
        PlayerPrefs.SetFloat("musicVolume", audioSrc.volume);
    }
}
