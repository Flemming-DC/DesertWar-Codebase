using System;
using UnityEngine;

[Serializable]
public class Sound
{
    public string name;
    public AudioClip audioClip;
    public bool isLooping = false;
    [Range(0, 1)] public float volume = 1;
    [Range(-3, 3)] public float pitch = 1;
    //[Range(0, 1)] public float spatialBlend = 1;

    [HideInInspector] public AudioSource audioSource;




}
