using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;


public class GameOverHandler : NetworkBehaviour
{
    public static event Action serverOnGameOver;
    public static event Action<string, bool> clientOnGameOver; // playerName, hasWon
    List<Base> serverBases = new List<Base>();
    Dictionary<int, int> serverBaseCounts = new Dictionary<int, int>();
    public static bool localGameOver;
    static bool gameOverForEveryone;
    public static GameOverHandler serverInstance { get; private set; }


    #region server

    public override void OnStartServer()
    {
        Base.serverOnBaseSpawned += ServerOnBaseSpawned;
        Base.serverOnBaseDespawned += ServerOnBaseDespawned;
        localGameOver = false;
        if (serverInstance != null)
            Debug.LogWarning($"singleton is already set");
        serverInstance = this;

        foreach(var player in NetManager.instance.players)
            serverBaseCounts.Add(player.connectionToClient.connectionId, 0);
    }


    public override void OnStopServer()
    {
        Base.serverOnBaseSpawned -= ServerOnBaseSpawned;
        Base.serverOnBaseDespawned -= ServerOnBaseDespawned;
    }

    [Server]
    void ServerOnBaseSpawned(Base base_)
    {
        serverBaseCounts[base_.connectionToClient.connectionId]++;
        serverBases.Add(base_);
    }


    [Server]
    void ServerOnBaseDespawned(Base base_)
    {
        serverBaseCounts[base_.connectionToClient.connectionId]--;

        if (serverBaseCounts[base_.connectionToClient.connectionId] == 0)
        {
            string loserName = base_.connectionToClient.identity.GetComponent<RTSPlayer>().displayName;
            ServerLose(base_.connectionToClient, loserName);
        }
        else if (serverBaseCounts[base_.connectionToClient.connectionId] < 0)
            Debug.LogWarning($"there are less than 0 bases left, but this should be impossible");


        
        serverBases.Remove(base_);

        if (serverBases.Count == 1)
        {
            string winnerName = serverBases[0].connectionToClient.identity.GetComponent<RTSPlayer>().displayName;
            ServerWin(winnerName);
        }
        else if (serverBases.Count == 0 && !localGameOver)
            ServerWin("None");
        else if (serverBases.Count < 0)
            Debug.LogWarning($"there are less than 0 bases left, but this should be impossible");
        
    }



    [Server]
    public void ServerLose(NetworkConnection conn, string playerName)
    {
        TargetLose(conn, playerName);
        // if you need to invoke this event, then you must tell it which player that lost, and only run gameOver code for that player.
        // serverOnGameOver?.Invoke(); 
        localGameOver = true;
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        player.hasLost = true;
        DeclareWinnerIfOneTeamStanding();
    }

    [Server]
    public void DeclareWinnerIfOneTeamStanding()
    {
        if (gameOverForEveryone)
            return;

        RTSPlayer[] survivors = NetManager.instance.humanPlayers.Where(p => !p.hasLost).ToArray();
        
        if (survivors.Length == 1)
            ServerWin(survivors[0].displayName);
        else if (survivors.Select(s => s.team).Distinct().Count() == 1)
            ServerWin($"Team {survivors[0].team}");
    }

    [Server]
    public void ServerWin(string playerName)
    {
        RpcWin(playerName);
        serverOnGameOver?.Invoke();
        localGameOver = true;
        gameOverForEveryone = true;
    }

    #endregion





    #region client

    [ClientRpc]
    void RpcWin(string playerName)
    {
        clientOnGameOver?.Invoke(playerName, true);
    }

    [TargetRpc]
    void TargetLose(NetworkConnection conn, string playerName)
    {
        clientOnGameOver?.Invoke(playerName, false);
    }

    #endregion







}
