using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class AudioManager : NetworkBehaviour
{
    [SerializeField] Settings settings;
    [SerializeField] Sound[] sounds;

    public static AudioManager instance { get; private set; }
    public static bool soundIsOn;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogWarning($"instance is already set. This is unexpected for a singleton");

        foreach(Sound sound in sounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            sound.audioSource = source;
            source.playOnAwake = false;
            source.loop = sound.isLooping;
            source.clip = sound.audioClip;
            source.volume = sound.volume;
            source.pitch = sound.pitch;
            //source.spatialBlend = sound.spatialBlend;
        }

        Invoke(nameof(EnableSound), 1);
    }

    void EnableSound() => soundIsOn = true;


    private void Update()
    {
        foreach (Sound sound in sounds)
            sound.audioSource.volume = settings.soundVolume * sound.volume;
    }
    
    public static void Play(string soundName)
    {
        if (!soundIsOn)
            return;

        Sound sound = Array.Find(instance.sounds, s => s.name == soundName);
        if (sound != null)
            sound.audioSource.Play();
        else
            Debug.LogWarning($"Can't find sound with name = {soundName}");
    }

    public static void Stop(string soundName)
    {
        Sound sound = Array.Find(instance.sounds, s => s.name == soundName);
        if (sound != null)
            sound.audioSource.Stop();
        else
            Debug.LogWarning($"Can't find sound with name = {soundName}");
    }

    [ClientRpc]
    public void RpcPlay(string soundName)
    {
        Play(soundName);
    }

    [ClientRpc]
    public void RpcStop(string soundName)
    {
        Stop(soundName);
    }

}
