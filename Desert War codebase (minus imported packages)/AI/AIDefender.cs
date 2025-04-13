using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AIDefender : MonoBehaviour
{
    [SerializeField] UnitCommander unitCommander;
    [SerializeField] SelectionManager unitSelectionHandler;

    AIAttacker aiAttacker;
    List<Selectable> defenceGroup = new List<Selectable>();
    RTSPlayer localPlayer;

    private void Start()
    {
        localPlayer = NetworkClient.localPlayer.GetComponent<RTSPlayer>();
        aiAttacker = GetComponent<AIAttacker>();
        BuildingHealth.clientOnSomeBuildingDamaged += Defend;
    }

    private void OnDisable()
    {
        BuildingHealth.clientOnSomeBuildingDamaged -= Defend;
    }

    void SetDefenceGroup()
    {
        defenceGroup.Clear();
        foreach(var unit in localPlayer.myUnits)
            if (!aiAttacker.attackGroup.Contains(unit))
                defenceGroup.Add(unit);
    }


    void Defend(Transform assaultedTransform)
    {
        if (!AIActivator.aiActivated)
            return;

        SetDefenceGroup();
        SelectDefenseGroup();
        AttackMoveToDefencePoint(assaultedTransform);
    }


    void AttackMoveToDefencePoint(Transform assaultedTransform)
    {
        Vector3 defencePoint = assaultedTransform.position + new Vector3(Random.Range(2f, 6f),
                                                                         0,
                                                                         Random.Range(2f, 6f));
        unitCommander.TryMove(defencePoint, true);
    }


    void SelectDefenseGroup()
    {
        unitSelectionHandler.selectedUnits.Clear();
        foreach (var unit in defenceGroup)
            unitSelectionHandler.AddToSelection(unit);
    }

}
