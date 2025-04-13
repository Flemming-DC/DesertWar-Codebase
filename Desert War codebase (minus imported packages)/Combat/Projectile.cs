using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour
{
    [SyncVar] [HideInInspector] public GameObject shooter;
    [SyncVar] [HideInInspector] public Vector3 targetPosition;

    protected Stats stats;
    protected int damage;
    protected float speed;
    protected float attackRange;
    protected Rigidbody rb;
    protected float lifeTime;
    protected GameObject explosionPrefab;
    float sqrMinFriendlyFireRange;
    Vector3 startPosition;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        stats = shooter.GetComponent<StatsBehaviour>().stats;
        damage = stats.damage;
        speed = stats.projectileSpeed;
        attackRange = stats.attackRange;
        sqrMinFriendlyFireRange = stats.minFriendlyFireRange * stats.minFriendlyFireRange;
        lifeTime = attackRange / speed * 1.05f;
        explosionPrefab = stats.projectileExplosion;

        rb.velocity = transform.forward * speed;
        startPosition = transform.position;
    }

    public override void OnStartServer()
    {
        StartCoroutine(DestroySelfWithDelay());
    }

    [ServerCallback]
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == shooter)
            return;
        if (other.TryGetComponent(out NetworkIdentity networkIdentity))
            if (networkIdentity.connectionToClient == connectionToClient)
            {
                float sqrDistance = (transform.position - startPosition).sqrMagnitude;
                if (sqrDistance < sqrMinFriendlyFireRange)
                    return;
                if (other.TryGetComponent(out Targetable targetable))
                    if (!targetable.canBeTargetedByAllies)
                        return;
            }
        if (other.TryGetComponent(out Health health))
            health.TakeDamage(damage);
        if (explosionPrefab != null && NetworkServer.active)
            this.Spawn(explosionPrefab, transform.position);

        NetworkServer.Destroy(gameObject);
    }

    [Server]
    IEnumerator DestroySelfWithDelay()
    {
        yield return new WaitForEndOfFrame();
        //lifeTime is being set later in this frame, therefore we skip a frame.
        yield return new WaitForSeconds(lifeTime);
        NetworkServer.Destroy(gameObject);
    }

}
