using System;
using UnityEngine;
using Mirror;

public class BuildingHealth : Health
{
    public static event Action<Transform> clientOnSomeBuildingDamaged;


    protected override void OnHealthUpdated(int OldHealth, int NewHealth)
    {
        base.OnHealthUpdated(OldHealth, NewHealth);
        if (NewHealth < OldHealth)
            clientOnSomeBuildingDamaged?.Invoke(transform);
    }

}
