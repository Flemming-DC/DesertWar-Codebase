using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AIAttacker : MonoBehaviour
{
    [Tooltip("the fraction of units that will participate in any given attack")]
    [SerializeField] float attackSize = 0.5f;
    [SerializeField] bool useMinutes;
    [SerializeField] float attackInterval = 2;
    [SerializeField] UnitCommander unitCommander;
    [SerializeField] SelectionManager unitSelectionHandler;

    public List<Selectable> attackGroup { get; private set; } = new List<Selectable>();
    Transform targetBase;
    RTSPlayer localPlayer;

    private void Start()
    {
        localPlayer = NetworkClient.localPlayer.GetComponent<RTSPlayer>();
        if (useMinutes)
            attackInterval *= 60;

        InvokeRepeating(nameof(Attack), attackInterval, attackInterval);
    }


    void Attack()
    {
        if (!AIActivator.aiActivated)
            return;

        SetTargetBase();
        SetAttackGroup();
        SelectAttackGroup();
        if (targetBase != null)
            AttackMoveToBase();
    }


    void AttackMoveToBase()
    {
        unitCommander.TryMove(targetBase.position, true);
        //unitCommander.TryTarget(targetBase.GetComponent<Targetable>());
    }


    void SelectAttackGroup()
    {
        unitSelectionHandler.selectedUnits.Clear();
        foreach (var unit in attackGroup)
            unitSelectionHandler.AddToSelection(unit);
    }

    void SetAttackGroup()
    {
        attackGroup.Clear();
        foreach(var unit in localPlayer.myUnits)
        {
            if (RandomBool(attackSize))
                attackGroup.Add(unit);
        }
    }


    void SetTargetBase()
    {
        foreach( var player in NetManager.instance.players)
        {
            if (player == localPlayer)
                continue;
            foreach (var building in Building.allBuildings)
                if (building.TryGetComponent(out Base base_))
                {
                    targetBase = base_.transform;
                    return;
                }
        }

        Debug.LogWarning($"Can't find enemy base");
    }

    bool RandomBool(float probability = 0.5f)
    {
        return (Random.value < probability);
    }


}
