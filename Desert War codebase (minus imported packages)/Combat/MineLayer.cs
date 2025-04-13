using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using TMPro;

public class MineLayer : NetworkBehaviour
{
    [SerializeField] GameObject minePrefab;
    [SerializeField] int mineCount = 3;

    [SyncVar] int remainingMineCount;

    public override void OnStartServer()
    {
        remainingMineCount = mineCount;
    }

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && hasAuthority)
            CmdPlaceMine();
    }

    [Command]
    void CmdPlaceMine()
    {
        if (remainingMineCount > 0)
        {
            this.Spawn(minePrefab, transform.position);
            remainingMineCount--;
            HintManager.SetHint($"{remainingMineCount} / {mineCount} mines left", connectionToClient);
            AudioManager.instance.RpcPlay("PlaceMine");
        }
        else
            HintManager.SetHint($"No mines left", connectionToClient, true);
    }


}


