using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public class RTSPlayer : NetworkBehaviour
{
    public float defaultBuildRange = 15;
    public float oilPumpMinBuildRange = 11;
    [SerializeField] Building[] allBuildingTypes;
    [SerializeField] LayerMask buildingBlockLayer;
    [SerializeField] int startingRessources = 300;
    [SerializeField] int editorStartingRessources = 3000;
    [SerializeField] GameObject dustPrefab;

    public List<Transform> owned { get; } = new List<Transform>();
    public List<Selectable> myUnits { get; } = new List<Selectable>();
    public List<Building> myBuildings { get; } = new List<Building>();
    public List<int> myBuildingIDs { get; } = new List<int>();
    public Color teamColor { get; set; }
    public GameObject baseObject { get; set; }
    public event Action<int> ClientOnRessourcesUpdated;
    public static Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerUpdated;
    [SyncVar(hook = nameof(ClientHandleRessourcesUpdated))] [HideInInspector] public int resources;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerUpdated))] [HideInInspector] public bool isPartyOwner = false;
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))] [HideInInspector] public string displayName;
    CameraController cameraController;
    public bool isRemoteServer { get => displayName == NetManager.instance.serverDisplayName; }
    public bool hasLost;
    public int team;


    #region both
    public bool CanPlaceBuilding(Building building, Vector3 position, bool inSight, out string message)
    {
        BoxCollider buildingCollider = building.GetComponent<BoxCollider>();
        bool positionIsBlocked = Physics.CheckBox(position + buildingCollider.center,
                                              buildingCollider.size / 2,
                                              Quaternion.identity,
                                              buildingBlockLayer);
        if (positionIsBlocked)
        {
            message = $"Cannot contruct {building.name} at invalid location.";
            return false;
        }
        if (!inSight)
        {
            message = $"Cannot contruct {building.name} outside the visible region.";
            return false;
        }
        if (!cameraController.PositionIsInMap(position))
        {
            message = $"Cannot contruct {building.name} outside the map.";
            return false;
        }

        if (building.isOilPump)
        {
            message = $"Cannot contruct an oil pump, so close to another oil pump.";
            return CheckOilPumpRange(position);
        }
        else
        {
            message = $"Cannot contruct {building.name}, so far from another building (excluding oil pumps).";
            return InRange(position);
        }
    }

    bool InRange(Vector3 position)
    {
        foreach (Building building_ in myBuildings)
        {
            if (!building_.isOilPump)
            {
                float sqrDistanceToBuilding = (position - building_.transform.position).sqrMagnitude;
                float sqrRange = defaultBuildRange * defaultBuildRange;
                if (sqrDistanceToBuilding <= sqrRange)
                    return true;
            }
        }
        return false;
    }

    bool CheckOilPumpRange(Vector3 position)
    {
        List<float> oilPumpSqrDistances = Building.allBuildings
                .Where(b => b.isOilPump)
                .Select(b => (position - b.transform.position).sqrMagnitude)
                .ToList();
        float minSqrRange = oilPumpMinBuildRange * oilPumpMinBuildRange;

        return oilPumpSqrDistances.All(d => d >= minSqrRange);
    }

    public Building GetBuildingFromID(int buildingID, Building[] buildingList)
    {
        Building building = null;
        foreach (Building building_ in buildingList)
        {
            if (building_.id == buildingID)
            {
                building = building_;
                break;
            }
        }
        return building;
    }

    void ResetUnitAndBuildingData()
    {
        myUnits.Clear();
        myBuildings.Clear();
        myBuildingIDs.Clear();
        owned.Clear();
    }

    public List<Transform> GetAlliedOwnables()
    {
        IEnumerable<Transform> alliedSelectables = (Selectable.allSelectables
            .Where(s => s.owner != null)
            .Where(s => s.owner.team == team)
            .Select(s => s.transform));
        IEnumerable<Transform> alliedBuildings = (Building.allBuildings
            .Where(s => s.owner != null)
            .Where(s => s.owner.team == team)
            .Select(s => s.transform));
        return alliedSelectables.Concat(alliedBuildings).ToList();
    }

    #endregion



    #region server
    public override void OnStartServer()
    {
        ResetUnitAndBuildingData();
        Selectable.serverOnUnitSpawned += ServerHandleUnitSpawned;
        Selectable.serverOnUnitDespawned += ServerHandleUnitDespawned;
        Building.serverOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.serverOnBuildingDespawned += ServerHandleBuildingDespawned;

        cameraController = GetComponent<CameraController>();
        DontDestroyOnLoad(gameObject);
        Invoke(nameof(LateStart), 2 * Time.deltaTime);
    }

    [Server]
    void LateStart()
    {
        resources = Application.isEditor ? editorStartingRessources : startingRessources;
    }

    public override void OnStopServer()
    {
        Selectable.serverOnUnitSpawned -= ServerHandleUnitSpawned;
        Selectable.serverOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.serverOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.serverOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    void ServerHandleUnitSpawned(Selectable selectable)
    {
        if (selectable.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myUnits.Add(selectable);
        owned.Add(selectable.transform);
    }

    void ServerHandleUnitDespawned(Selectable selectable)
    {
        if (selectable.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myUnits.Remove(selectable);
        owned.Remove(selectable.transform);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId == connectionToClient.connectionId)
        {
            myBuildings.Add(building);
            myBuildingIDs.Add(building.id);
            owned.Add(building.transform);
        }
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId == connectionToClient.connectionId)
        {
            myBuildings.Remove(building);
            myBuildingIDs.Remove(building.id);
            owned.Remove(building.transform);
        }
    }

    [Command]
    public void CmdStartGame()
    {
        NetManager.instance.StartGame();
        /*
        if (isPartyOwner || AIActivator.aiActivated || NetData.isSinglePlayer)
            NetManager.instance.StartGame();
        */
    }

    [Command]
    public void CmdSetDisplayName(string newName)
    {
        displayName = newName;
    }


    [Command]
    public void CmdSetResources(int newResources)
    {
        resources = newResources;
    }


    [Command]
    public void CmdSetTeam(int newTeam)
    {
        team = newTeam;
        ClientRpcSetTeam(newTeam);
    }



    #endregion




    #region client

    public override void OnStartClient()
    {
        if (isServer)
            return;

        cameraController = GetComponent<CameraController>();
        NetManager.instance.players.Add(this);
        DontDestroyOnLoad(gameObject);

        if (!hasAuthority)
            return;

        ResetUnitAndBuildingData();
        Selectable.authorityOnUnitSpawned += AuthoritytHandleUnitSpawned;
        Selectable.authorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        Building.authorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.authorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStopClient()
    {
        if (isServer)
        {
            ClientOnInfoUpdated?.Invoke();
            return;
        }

        NetManager.instance.players.Remove(this);
        ClientOnInfoUpdated?.Invoke();

        if (!hasAuthority)
            return;
        

        Selectable.authorityOnUnitSpawned -= AuthoritytHandleUnitSpawned;
        Selectable.authorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.authorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.authorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }


    void AuthoritytHandleUnitSpawned(Selectable selectable)
    {
        myUnits.Add(selectable);
        owned.Add(selectable.transform);
    }

    void AuthorityHandleUnitDespawned(Selectable selectable)
    {
        myUnits.Remove(selectable);
        owned.Remove(selectable.transform);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
        myBuildingIDs.Add(building.id);
        owned.Add(building.transform);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
        myBuildingIDs.Remove(building.id);
        owned.Remove(building.transform);
    }


    void AuthorityHandlePartyOwnerUpdated(bool oldIsPartyOwner, bool newIsPartyOwner)
    {
        if (hasAuthority)
            AuthorityOnPartyOwnerUpdated?.Invoke(newIsPartyOwner);
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingID, bool inSight, Vector3 position)
    {
        Building building = GetBuildingFromID(buildingID, allBuildingTypes);
        if (building == null)
        {
            HintManager.SetHint($"Cannot find the building to be placed", connectionToClient, true);
            return;
        }

        if (resources < building.cost)
        {
            HintManager.SetHint($"Not enough resources to contruct {building.name}.", connectionToClient, true);
            return;
        }

        if (!CanPlaceBuilding(building, position, inSight, out string message))
        {
            HintManager.SetHint(message, connectionToClient, true);
            return;
        }

        this.Spawn(dustPrefab, position);
        this.Spawn(building.gameObject, position);
        resources -= building.cost;
    }

    void ClientHandleRessourcesUpdated(int oldRessources, int newRessources)
    {
        ClientOnRessourcesUpdated?.Invoke(newRessources);
    }

    void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }


    [ClientRpc]
    public void ClientRpcSetTeam(int newTeam)
    {
        team = newTeam;
        LobbyMenu.instance.OnTeamChanged(this);
    }


    #endregion

}
