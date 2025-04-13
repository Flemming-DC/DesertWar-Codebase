using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Mirror;

public class ExplosionTester : NetworkBehaviour
{
    [SerializeField] GameObject[] explosionPrefabs;
    
    Camera mainCamera;
    List<KeyControl> digitKeys = new List<KeyControl>();

    private void Start()
    {
        mainCamera = Camera.main;
        MakeDigitKeys();
    }
    
    void Update()
    {
        for(int i=0; i<digitKeys.Count; i++)
        {
            if (digitKeys[i].isPressed)
                MakeExplosion(i);
        }
    }


    void MakeExplosion(int i)
    {
        if (i > explosionPrefabs.Length)
        {
            Debug.LogWarning($"There are only {explosionPrefabs.Length}, but you are trying to access the {i}'th element.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Physics.Raycast(ray, out RaycastHit hit);
        this.Spawn(explosionPrefabs[i], hit.point);
        Debug.Log($"Explosion: {explosionPrefabs[i]}");
    }

    void MakeDigitKeys()
    {
        digitKeys.Add(Keyboard.current.digit1Key);
        digitKeys.Add(Keyboard.current.digit2Key);
        digitKeys.Add(Keyboard.current.digit3Key);
        digitKeys.Add(Keyboard.current.digit4Key);
        digitKeys.Add(Keyboard.current.digit5Key);
        digitKeys.Add(Keyboard.current.digit6Key);
        digitKeys.Add(Keyboard.current.digit7Key);
        digitKeys.Add(Keyboard.current.digit8Key);
        digitKeys.Add(Keyboard.current.digit9Key);
    }

}
