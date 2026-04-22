using System;
using UnityEngine;
using UnityEngine.Rendering;

public class LoopingAudio : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] AudioSource audioSource;
    [SerializeField] float volume = 1f;
    void Start()
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopSound()
    {
        audioSource.Stop();
    }
}