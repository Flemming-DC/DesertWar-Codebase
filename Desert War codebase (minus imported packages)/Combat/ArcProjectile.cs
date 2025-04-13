using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class ArcProjectile : Projectile
{
    [SerializeField] Transform destinationMarker;

    float gravity;
    float initialVerticalVelocity;

    protected override void Start()
    {
        base.Start();
        InitializeVelocity();
        InitializeGravityAndLifeTime();
        InitializeDestinationMarker();
    }


    private void FixedUpdate()
    {
        rb.AddForce(gravity * Vector3.down, ForceMode.Acceleration);
        transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    void InitializeVelocity()
    {
        Vector3 firingDirection = transform.forward.Horizontal();
        float firingAngle = stats.firingAngle;
        Vector3 RotationAxis = new Vector3(-firingDirection.z, 0, firingDirection.x);
        firingDirection = Quaternion.AngleAxis(firingAngle, RotationAxis) * firingDirection;
        rb.velocity = firingDirection * speed;
        initialVerticalVelocity = rb.velocity.y;
    }

    void InitializeGravityAndLifeTime()
    {
        float forwardVelocity = Vector3.Dot(rb.velocity, transform.forward.Horizontal().normalized);
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        gravity = Mathf.Abs(2 * forwardVelocity * rb.velocity.y / distanceToTarget);
        lifeTime = distanceToTarget / forwardVelocity * 1.05f;
    }

    void InitializeDestinationMarker()
    {
        destinationMarker.parent.SetParent(null);
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        destinationMarker.position = targetPosition - 0.1f * ray.direction;
        destinationMarker.localScale = 1.25f * stats.AOERange * new Vector3(1, 1, 1);
    }
    
    [ServerCallback]
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == shooter)
            return;
        // explode when projectile lands on top of target, not when passes a unit on its way upwards
        if (rb.velocity.y > 0.5f * initialVerticalVelocity)
            return;

        NetworkServer.Destroy(gameObject);
    }

    
    void OnDestroy()
    {
        if (destinationMarker != null)
            Destroy(destinationMarker.gameObject);

        if (!isServer)
            return;
        if (!NetworkServer.active)
            return;

        DealDamageWithAOE();

        if (explosionPrefab != null && NetworkServer.active)
            this.Spawn(explosionPrefab, transform.position);
    }

    [Server]
    void DealDamageWithAOE()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, stats.AOERange);

        foreach (Collider collider in nearbyColliders)
        {
            if (collider.TryGetComponent(out NetworkIdentity networkIdentity))
                if (networkIdentity.connectionToClient == connectionToClient)
                    if (collider.TryGetComponent(out Targetable targetable))
                        if (!targetable.canBeTargetedByAllies)
                            continue;
            if (collider.TryGetComponent(out Health health))
                health.TakeDamage(damage);
        }
    }

}
