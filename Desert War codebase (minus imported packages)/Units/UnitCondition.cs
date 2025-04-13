using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class UnitCondition : NetworkBehaviour
{
    public bool isOccupied { get; set; }
    public bool isStunned { get; private set; }

    NavMeshAgent agent;
    Stats stats;

    public override void OnStartServer()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<StatsBehaviour>().stats;
    }

    [Server]
    public void Stun(float duration)
    {
        isStunned = true;
        agent.ResetPath();
        agent.speed = 0;
        Invoke(nameof(UnStun), duration);
    }

    void UnStun()
    {
        isStunned = false;
        agent.speed = stats.movementSpeed;
    }

}
