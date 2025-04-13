using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targetable : NetworkBehaviour
{
    [field: SerializeField] public Transform aimTransform { get; private set; }
    public bool isAutoTargetable { get; private set; }
    public bool canBeTargetedByAllies { get; private set; }

    public List<Targeter> targeters { get; set; } = new List<Targeter>();

    private void Awake()
    {
        Stats stats = GetComponent<StatsBehaviour>().stats;
        isAutoTargetable = stats.isAutoTargetable;
        canBeTargetedByAllies = stats.canBeTargetedByAllies;
    }

    private void OnDestroy()
    {
        if (!NetworkServer.active)
            return;

        foreach (Targeter targeter in targeters)
        {
            if (targeter != null) 
                targeter.ClearTarget();
        }
    }

}
