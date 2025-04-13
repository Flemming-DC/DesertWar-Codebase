using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnAwake : MonoBehaviour
{
    [SerializeField] string[] soundNames;

    private void Awake()
    {
        foreach (string soundName in soundNames)
            AudioManager.Play(soundName);
    }

}
