using System;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[SelectionBase]
[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Targeter targeter;
    [Tooltip("this value should be slightly less than 1, to ensure that the unit stops chasing, when the target is slightly inside the attackRange.")]
    [SerializeField] float safetyFactorForChaseStoppingRange = 0.95f;

    UnitCondition unitCondition;
    UnitFiring unitFiring;
    string movementSound;
    float sqrChaseStoppingRange;
    float attackRange;

    #region server
    [ServerCallback]
    private void Start()
    {
        Stats stats = GetComponent<StatsBehaviour>().stats;
        unitCondition = GetComponent<UnitCondition>();
        unitFiring = GetComponent<UnitFiring>();
        agent.speed = stats.movementSpeed;
        attackRange = stats.attackRange;
        movementSound = stats.movementSound;
        sqrChaseStoppingRange = Mathf.Pow(safetyFactorForChaseStoppingRange * attackRange, 2);

        if (stats == null)
            Debug.LogWarning($"{name}.UnitMovement failed to find stats {stats}");
        if (unitCondition == null)
            Debug.LogWarning($"{unitCondition}.UnitMovement failed to find unitCondition {unitCondition}");
    }

    [ServerCallback]
    private void Update()
    {
        Targetable targetable = targeter.target;

        if (targetable != null)
        {
            Chase(targetable);
        }
        else if (agent.hasPath && agent.remainingDistance < agent.stoppingDistance)
        {
            //this line prevents the units from continuing to push each other away from the destination.
            agent.ResetPath();
            unitCondition.isOccupied = false;
            if (movementSound != "")
                AudioManager.instance.RpcStop(movementSound);
        }
            
    }

    [Server]
    void Chase(Targetable targetable)
    {
        bool isWithinChaseStoppingRange = (targetable.transform.position - transform.position).sqrMagnitude < sqrChaseStoppingRange;
        if (!isWithinChaseStoppingRange)
            agent.SetDestination(targetable.transform.position);
        else if (agent.hasPath)
            agent.ResetPath();
    }

    [Server]
    public void ServerMove(Vector3 destination, bool isAttackMoving = false)
    {
        if (unitCondition == null)
            return;

        if (!isAttackMoving)
            unitFiring.CancelShot();
        targeter.ClearTarget();
        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 3, NavMesh.AllAreas))
        {
            HintManager.SetHint("Can't move to that location.", connectionToClient);
            print($"Can't move to location: {destination}");
            return;
        }
        if (movementSound != "")
            AudioManager.instance.RpcPlay(movementSound);
        agent.SetDestination(hit.position);
        if (!isAttackMoving)
        {
            unitCondition.isOccupied = true;
            targeter.isAttackMoving = false;
        }
        else
            targeter.isAttackMoving = true;
    }

    [Command]
    public void CmdMove(Vector3 destination, bool isAttackMoving)
    {
        ServerMove(destination, isAttackMoving);
    }


    [Command]
    public void CmdCancel()
    {
        if (unitCondition == null)
            return;

        unitFiring.CancelShot();
        targeter.ClearTarget();
        agent.SetDestination(transform.position);
        unitCondition.isOccupied = false;
        targeter.isAttackMoving = false;
    }
    
    #endregion





}
