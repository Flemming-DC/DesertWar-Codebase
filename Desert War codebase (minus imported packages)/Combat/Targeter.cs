using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class Targeter : NetworkBehaviour
{
    public bool isAttackMoving { private get; set; }
    public Targetable target { get; private set; }

    Targetable thisTargetable;
    UnitCondition unitCondition;
    UnitFiring unitFiring;
    float aggresionRange;
    float attackSpeed;
    float attackRange;
    bool isMovable;

    #region server

    public override void OnStartServer()
    {
        Base.serverOnPlayerLose += ServerOnPlayerLose;
        aggresionRange = GetComponent<StatsBehaviour>().stats.aggresionRange;
        attackRange = GetComponent<StatsBehaviour>().stats.attackRange;
        unitCondition = GetComponent<UnitCondition>();
        thisTargetable = GetComponent<Targetable>();
        unitFiring = GetComponent<UnitFiring>();
        attackSpeed = GetComponent<StatsBehaviour>().stats.attackSpeed;
        isMovable = TryGetComponent(out NavMeshAgent _);
    }


    public override void OnStopServer()
    {
        Base.serverOnPlayerLose -= ServerOnPlayerLose;
    }


    [ServerCallback]
    private void Update()
    {
        if (unitCondition != null)
            if (unitCondition.isOccupied || unitCondition.isStunned)
                return;
        if (target != null)
        {
            if (!FogOfWar.InSight(target.transform.position, this.ServerGetPlayer()))
                ClearTarget();
            else if (!isMovable && !unitFiring.InRange(out Vector3 _))
                ClearTarget();
            else
                return;
        }
        if (Time.time > unitFiring.lastAttackTime + 1 / attackSpeed)
            AutoTarget();
    }

    [Server]
    void AutoTarget()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, aggresionRange);
        foreach (var nearbyCollider in nearbyColliders)
        {
            RTSPlayer targetersOwner = this.ServerGetPlayer();
            if (targetersOwner == null)
                continue;
            if (!FogOfWar.InSight(nearbyCollider.transform.position, targetersOwner))
                continue;
            if (nearbyCollider.TryGetComponent(out Targetable nearbyTargetable))
            {
                if (!nearbyTargetable.isAutoTargetable)
                    continue;
                //if (connectionToClient.connectionId == nearbyTargetable.connectionToClient.connectionId)
                //    continue;
                if (nearbyTargetable.ServerGetPlayer() == null)
                    continue;
                if (targetersOwner.team == nearbyTargetable.ServerGetPlayer().team)
                    continue;

                ServerSetTarget(nearbyTargetable);
                break;
            }
        }
    }

    [Command]
    public void CmdSetTarget(Targetable newTargetable)
    {
        ServerSetTarget(newTargetable);
    }

    [Server]
    void ServerSetTarget(Targetable newTarget)
    {
        if (newTarget == target)
            return;
        if (newTarget == thisTargetable && thisTargetable != null)
            return;
        
        unitFiring.CancelShot();
        target = newTarget;
        if (!isAttackMoving && unitCondition != null)
            unitCondition.isOccupied = true;
        if (target != null)
            target.targeters.Add(this);
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
        if (unitCondition != null)
            unitCondition.isOccupied = false;
        Invoke(nameof(InformTargetableOfClearingTarget), Time.deltaTime);
    }

    [Server]
    void InformTargetableOfClearingTarget()
    {
        if (target != null)
            target.targeters.Remove(this);
    }

    private void ServerOnPlayerLose(int obj)
    {
        target = null;
    }


    #endregion


}
