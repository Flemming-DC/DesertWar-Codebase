using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Health))]
public class Base : NetworkBehaviour
{
    public static event Action<Base> serverOnBaseSpawned;
    public static event Action<Base> serverOnBaseDespawned;
    public static event Action<Base> authorityOnBaseSpawned;
    public static event Action<Base> authorityOnBaseDespawned;
    public static event Action<int> serverOnPlayerLose;

    public static Dictionary<NetworkConnection, int> baseCounts { get; private set; } = new Dictionary<NetworkConnection, int>();
    public static List<Base> authorityBases { get; private set; } = new List<Base>();
    Health health;


    #region server

    public override void OnStartServer()
    {
        health = GetComponent<Health>();
        health.serverOnDie += ServerHandleDie;
        serverOnBaseSpawned?.Invoke(this);
    }
    public override void OnStopServer()
    {
        health.serverOnDie -= ServerHandleDie;
        serverOnBaseDespawned?.Invoke(this);
    }

    [Server]
    void ServerHandleDie()
    {
        baseCounts[connectionToClient]--;
        if (baseCounts[connectionToClient] <= 0)
            serverOnPlayerLose?.Invoke(connectionToClient.connectionId);
    }

    [Command]
    void CmdAddBase()
    {
        if (!baseCounts.ContainsKey(connectionToClient))
            baseCounts.Add(connectionToClient, 0);
        baseCounts[connectionToClient] += 1;
    }

    [Command]
    public void Surrender()
    {
        serverOnPlayerLose?.Invoke(connectionToClient.connectionId);
    }

    #endregion



    #region client

    public override void OnStartClient()
    {
        if (!hasAuthority)
            return;
        authorityOnBaseSpawned?.Invoke(this);
        authorityBases.Add(this);
        CmdAddBase();
    }

    public override void OnStopClient()
    {
        if (!hasAuthority)
            return;
        authorityOnBaseDespawned?.Invoke(this);
        authorityBases.Remove(this);
    }



    #endregion



}
