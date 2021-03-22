using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource bgm, victory;

    public AudioSource[] soundEffects;

    private void Awake()
    {
        instance = this;
    }

    public void StopBGM()
    {
        bgm.Stop();
    }

    public void PlayLevelVictory()
    {
        StopBGM();
        victory.Play();
    }

    public void PlaySFX(int sfxNumber)
    {
        soundEffects[sfxNumber].Stop();
        soundEffects[sfxNumber].Play();
    }

    public void StopSFX(int sfxNumber)
    {
        soundEffects[sfxNumber].Stop();
    }
}
