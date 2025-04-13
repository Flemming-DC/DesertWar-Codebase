using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Mine : NetworkBehaviour
{
    [SerializeField] GameObject explosionPrefab;

    bool mineLayerHasLeft = false;

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!mineLayerHasLeft)
            return;
        if (!other.TryGetComponent<Health>(out Health otherHealth))
            return;

        int damage = GetComponent<StatsBehaviour>().stats.damage;
        otherHealth.TakeDamage(damage);
        this.Spawn(explosionPrefab, transform.position);
        NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        mineLayerHasLeft = true;
    }

}
