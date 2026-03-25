using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer & Groups")]
    public AudioMixer mainMixer;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip coinSound;
    public AudioClip buttonClick;
    public AudioClip gameMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Звук не прерывается при смене сцен
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        musicSource.clip = gameMusic;
        musicSource.Play();
        SetMenuMode(true); // Начинаем в режиме меню
    }

    // Тот самый переход: приглушение музыки
    public void SetMenuMode(bool isMenu)
    {
        // 22000 Гц - звук чистый, 800 Гц - звук приглушенный
        float freq = isMenu ? 800f : 22000f;
        mainMixer.SetFloat("MusicLowPass", freq);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
          sfxSource.PlayOneShot(clip);
        }
        
    }
}
