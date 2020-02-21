using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSystem : MonoBehaviour
{
    public AudioSource musicPlayer;
    public AudioSource ambientPlayer;
    public AudioClip[] ambientMusic;
    public AudioClip battleMusic;
    int currentSong = 0;
    int timeStop = 0;
    float musicMultiplier = 1.0f;
    float ambientMultiplier = 1.0f;
    float backgroundBase = 0.05f;
    float ambientBase = 0.5f;
    float battleBase = 0.45f;
    bool inBattle = false;
    Coroutine musicSwapper;

    void Start()
    {
        musicPlayer.volume = ambientBase;
        ambientPlayer.volume = backgroundBase;
        currentSong = Random.Range(0, ambientMusic.Length);
        musicPlayer.clip = ambientMusic[currentSong];
        musicPlayer.Play();
        musicSwapper = StartCoroutine(musicPlayerHandler());
    }

    IEnumerator musicPlayerHandler()
    {
        while (true)
        {
            while (musicPlayer.isPlaying)
            {
                yield return null;
            }

            float time = musicPlayer.time;
            if (!inBattle && time >= musicPlayer.clip.length)
            {
                currentSong++;
                if (currentSong >= ambientMusic.Length)
                {
                    currentSong = 0;
                }
                musicPlayer.clip = ambientMusic[currentSong];
                musicPlayer.timeSamples = 0;
                musicPlayer.Play();
            }
            yield return null;
        }
    }

    public void volumeSlider(Slider slider)
    {
        musicMultiplier = slider.value;
        if (inBattle)
        {
            musicPlayer.volume = battleBase * musicMultiplier;
        } else
        {
            musicPlayer.volume = ambientBase * musicMultiplier;
        }
    }

    public void ambientSlider(Slider slider)
    {
        ambientMultiplier = slider.value;
        if (!inBattle)
        {
            ambientPlayer.volume = backgroundBase * ambientMultiplier;
        }
    }

    public void PlayAmbientMusic()
    {
        inBattle = false;
        ambientPlayer.volume = backgroundBase * ambientMultiplier;
        musicPlayer.loop = false;
        StartCoroutine(switchSong(ambientMusic[currentSong], timeStop, ambientBase * musicMultiplier));
    }

    public void PlayBattleMusic()
    {
        timeStop = musicPlayer.timeSamples;
        musicPlayer.loop = true;
        musicPlayer.volume = 0;
        ambientPlayer.volume = 0;
        inBattle = true;
        StartCoroutine(switchSong(battleMusic, 0, battleBase * musicMultiplier, 3));
    }

    IEnumerator switchSong(AudioClip audioClip, int playPoint, float volume, float timeScale = 1)
    {
        while (musicPlayer.volume > 0)
        {
            musicPlayer.volume -= Time.deltaTime / timeScale;
            yield return null;
        }

        musicPlayer.clip = audioClip;
        musicPlayer.timeSamples = playPoint;
        musicPlayer.Play();

        while(musicPlayer.volume < volume) {
            musicPlayer.volume += Time.deltaTime / timeScale;
            yield return null;
        }
        yield return null;
    }
}
