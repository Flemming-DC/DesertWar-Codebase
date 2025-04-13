using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LifetimeLimiter : NetworkBehaviour
{
    [SerializeField] float lifetime;

    public override void OnStartServer()
    {
        Invoke(nameof(Die), lifetime);
    }

    [Server]
    void Die()
    {
        NetworkServer.Destroy(gameObject);
    }


}
