using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitHealth : Health
{

    #region server
    [Server]
    protected override void ServerOnDie()
    {
        string movementSound = GetComponent<StatsBehaviour>().stats.movementSound;
        if (movementSound != "")
            AudioManager.instance.RpcStop(movementSound);
        base.ServerOnDie();
    }
    #endregion
}
