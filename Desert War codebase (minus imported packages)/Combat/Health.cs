using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Health : NetworkBehaviour
{

    [SyncVar(hook = nameof(OnHealthUpdated))] protected int currentHealth;
    public event Action<int, int> clientOnHealthUpdated;
    public event Action serverOnDie;
    [SyncVar] int maxHealth;
    GameObject explosionPrefab;

    #region server

    public override void OnStartServer()
    {
        Stats stats = GetComponent<StatsBehaviour>().stats;
        maxHealth = stats.maxHealth;
        explosionPrefab = stats.deathExplosion;
        currentHealth = maxHealth;
        serverOnDie += ServerOnDie;
        Base.serverOnPlayerLose += ServerOnPlayerLose;
    }

    public override void OnStopServer()
    {
        serverOnDie -= ServerOnDie;
        Base.serverOnPlayerLose -= ServerOnPlayerLose;
    }

    [Server]
    protected virtual void ServerOnDie()
    {
        bool mapIsLoading = Time.timeSinceLevelLoad < NetData.mapLoadingTime + 1;
        bool isServerSurrenderSound = mapIsLoading && NetData.isRemoteServer;
        if (explosionPrefab != null && NetworkServer.active && !isServerSurrenderSound)
            this.Spawn(explosionPrefab, transform.position);

        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;
        if (currentHealth <= 0)
            serverOnDie?.Invoke();
    }

    [Server]
    private void ServerOnPlayerLose(int playerID)
    {
        if(connectionToClient.connectionId == playerID)
        {
            TakeDamage(maxHealth);
        }
    }

    #endregion



    #region client
    protected virtual void OnHealthUpdated(int dummyOldHealth, int dummyNewHealth)
    {
        clientOnHealthUpdated?.Invoke(maxHealth, currentHealth);
    }

    #endregion
}
