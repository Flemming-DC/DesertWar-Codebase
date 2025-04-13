using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] int resourcesPerInterval;
    [SerializeField] float interval;

    float timer;
    RTSPlayer player;
    Health health;

    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();
        health = GetComponent<Health>();

        health.serverOnDie += ServerOnDie;
        GameOverHandler.serverOnGameOver += ServerOnGameOver;
    }

    public override void OnStopServer()
    {
        health.serverOnDie -= ServerOnDie;
        GameOverHandler.serverOnGameOver -= ServerOnGameOver;
    }

    [ServerCallback]
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = interval;
            player.resources += resourcesPerInterval;
        }
    }


    private void ServerOnGameOver()
    {
        enabled = false;
    }

    private void ServerOnDie()
    {
        Destroy(gameObject);
    }




}
