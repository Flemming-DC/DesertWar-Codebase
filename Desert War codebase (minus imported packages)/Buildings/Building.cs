using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Building : NetworkBehaviour
{
    [field: SerializeField] public GameObject buildingPreview { get; private set; }
    [field: SerializeField] public int id { get; private set; } = -1;
    [field: SerializeField] public float heightOfReferencePosition { get; private set; } = 0;

    [SyncVar] public RTSPlayer owner;
    public static event Action<Building> serverOnBuildingSpawned;
    public static event Action<Building> serverOnBuildingDespawned;
    public static event Action<Building> authorityOnBuildingSpawned;
    public static event Action<Building> authorityOnBuildingDespawned;
    public int cost { get { return GetComponent<StatsBehaviour>().stats.cost; } }
    public bool isOilPump { get => gameObject.name.ToLower().Contains("pump"); }
    static bool hasAlreadyABuilding = false;
    public static List<Building> allBuildings = new List<Building>();

    #region Server

    public override void OnStartServer()
    {
        serverOnBuildingSpawned?.Invoke(this);
    }


    public override void OnStopServer()
    {
        serverOnBuildingDespawned?.Invoke(this);
    }

    [Command]
    void SetOwner(RTSPlayer player)
    {
        if (owner != null)
            Debug.LogWarning($"owner is already set.");
        owner = player;
    }

    #endregion




    #region Client

    public override void OnStartClient()
    {
        allBuildings.Add(this);
        if (!hasAuthority)
            return;
        SetOwner(NetworkClient.localPlayer.GetComponent<RTSPlayer>());
        authorityOnBuildingSpawned?.Invoke(this);

        if (hasAlreadyABuilding)
            AudioManager.Play("PlaceBuilding");
        else
            hasAlreadyABuilding = true;
    }


    public override void OnStopClient()
    {
        allBuildings.Remove(this);
        if (hasAuthority)
            authorityOnBuildingDespawned?.Invoke(this);
    }


    public bool HasRequiredBuildings(RTSPlayer player)
    {
        foreach (Building building in GetComponent<StatsBehaviour>().stats.requirements)
        {
            if (!player.myBuildingIDs.Contains(building.id))
                return false;
        }
        return true;
    }

    #endregion

}
