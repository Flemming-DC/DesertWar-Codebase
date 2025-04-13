using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Selectable : NetworkBehaviour
{
    [field: SerializeField] public int cost { get; private set; } = 50;
    [field: SerializeField] public UnitMovement unitMovement { get; private set; }
    [field: SerializeField] public Targeter targeter { get; private set; }
    [SerializeField] UnityEvent onSelected;
    [SerializeField] UnityEvent onDeselected;

    [SyncVar] public RTSPlayer owner;
    public static event Action<Selectable> serverOnUnitSpawned;
    public static event Action<Selectable> serverOnUnitDespawned;
    public static event Action<Selectable> authorityOnUnitSpawned;
    public static event Action<Selectable> authorityOnUnitDespawned;
    public static List<Selectable> allSelectables = new List<Selectable>();


    #region server
    public override void OnStartServer()
    {
        serverOnUnitSpawned?.Invoke(this);
        Deselect();
    }
    public override void OnStopServer()
    {
        serverOnUnitDespawned?.Invoke(this);
    }

    [Command]
    void SetOwner(RTSPlayer player)
    {
        if (owner != null)
            Debug.LogWarning($"owner is already set.");
        owner = player;
    }
    #endregion



    #region client
    [Client]
    public void Select()
    {
        if (!hasAuthority)
            return;
        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority)
            return;
        onDeselected?.Invoke();
    }

    public override void OnStartClient()
    {
        allSelectables.Add(this);
        if (!hasAuthority)
            return;

        SetOwner(NetworkClient.localPlayer.GetComponent<RTSPlayer>());
        authorityOnUnitSpawned?.Invoke(this);
    }
    public override void OnStopClient()
    {
        allSelectables.Remove(this);
        if (!hasAuthority)
            return;

        authorityOnUnitDespawned?.Invoke(this);
    }
    #endregion
}
