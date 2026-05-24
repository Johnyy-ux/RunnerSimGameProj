using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip coinSound;
    public AudioClip jumpSound;
    public AudioClip buttonClick;
    public AudioClip gameMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (musicSource && gameMusic)
        {
            musicSource.clip = gameMusic;
            musicSource.Play();
        }

        SetMenuMode(true);
    }

    /// <summary>
    /// Muffles music in menu, full range in game
    /// </summary>
    public void SetMenuMode(bool isMenu)
    {
        if (mainMixer == null) return;
        float freq = isMenu ? 800f : 22000f;
        mainMixer.SetFloat("MusicLowPass", freq);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayJumpSound() => PlaySFX(jumpSound);
    public void PlayButtonClick() => PlaySFX(buttonClick);

}
