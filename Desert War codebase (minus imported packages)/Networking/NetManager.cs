using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Linq;

public class NetManager : NetworkManager
{
    [Space(30)]
    [Header("RTS Settings")]
    [SerializeField] GameObject basePrefab;
    [SerializeField] GameOverHandler gameOverHandlerPrefab;
    [SerializeField] string twoPlayerMap;
    [SerializeField] string threePlayerMap;
    [SerializeField] string fourPlayerMap;
    [field: SerializeField] public int minPlayerCount { get; private set; } = 1;
    [SerializeField] List<Color> teamColors;


    public List<RTSPlayer> humanPlayers { get => players.Where(p => !p.isRemoteServer).ToList(); }
    public static NetManager instance { get => (NetManager)singleton; }
    public List<RTSPlayer> players { get; } = new List<RTSPlayer>();
    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;
    //public static event Action<NetworkConnection> ServerOnClientDisconnected;
    bool isGameInProgress = false;
    public string serverDisplayName {get => "None";}

    #region server
    public void OnServerInitialized()
    {
        if (teamColors.Count < maxConnections)
            Debug.LogWarning("there must be at least as many team colors as the maximum number of connections.");
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (isGameInProgress)
            conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        players.Remove(player);
        if (LobbyMenu.instance != null)
            LobbyMenu.instance.ClientHandleInfoUpdated();
        teamColors.Add(player.teamColor);
        GameOverHandler.serverInstance?.DeclareWinnerIfOneTeamStanding();
        this.Delay(() => DestroyPlayer(player), 2 * Time.deltaTime);
    }

    void DestroyPlayer(RTSPlayer player)
    {
        if (player != null)
            NetworkServer.Destroy(player.gameObject);
    }

    public override void OnStopServer()
    {
        players.Clear();
        isGameInProgress = false;
    }

    public void StartGame()
    {
        if (players.Count >= minPlayerCount)
        {
            isGameInProgress = true;
            if (humanPlayers.Count == 2)
                ServerChangeScene(twoPlayerMap);
            else if (humanPlayers.Count == 3)
                ServerChangeScene(threePlayerMap);
            else
                ServerChangeScene(fourPlayerMap);
        }

    }

    
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        players.Add(player);
        player.isPartyOwner = (players.Count == 1);
        player.teamColor = teamColors[UnityEngine.Random.Range(0, teamColors.Count)];
        teamColors.Remove(player.teamColor);
        player.displayName = $"Player {humanPlayers.Count}";
        player.team = humanPlayers.Count;
        foreach (var p in humanPlayers)
            p.ClientRpcSetTeam(p.team);
        if (NetData.isRemoteServer)
        {
            player.displayName = player.isPartyOwner ? serverDisplayName : $"Player {humanPlayers.Count}";
            maxConnections++;
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        bool isMapNotMenu = !SceneManager.GetActiveScene().name.ToLower().Contains("menu");
        if (isMapNotMenu)
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
            foreach(RTSPlayer player in players)
            {
                Vector3 startPosition = GetStartPosition(player).position;
                player.transform.position = startPosition;
                GameObject baseInstance = Instantiate(basePrefab, startPosition, Quaternion.identity);
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
                player.baseObject = baseInstance;
            }
        }

        
    }

    Transform GetStartPosition(RTSPlayer player)
    {
        startPositions.RemoveAll(t => t == null);
        if (playerSpawnMethod == PlayerSpawnMethod.RoundRobin)
            Debug.LogWarning($"The StartPosition functionality has been replaced, so that round robin is no longer implemented. Yet you have chosen round robin. This seems wrong.");
        if (startPositions.Count == 0)
        {
            Debug.LogWarning($"Cannot get startPosition because there aren't any.");
            return null;
        }

        if (player.isRemoteServer)
            return startPositions[0]; // the start positions are ordered by sibling index, so the server startPosition must have the lowest index.
        else
        {
            Transform startPosition = startPositions[UnityEngine.Random.Range(1, startPositions.Count)];
            startPositions.Remove(startPosition);
            return startPosition;
        }

    }


    #endregion


    #region client
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        ClientOnDisconnected?.Invoke();
        if (LobbyMenu.instance != null)
            LobbyMenu.instance.ClientHandleInfoUpdated();
    }

    public override void OnStopClient()
    {
        players.Clear();
    }


    #endregion




}
