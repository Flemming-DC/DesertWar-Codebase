using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] GameObject projetilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float rotationSpeed = 180;
    [SerializeField] float minAngleToTarget = 30;
    [SerializeField] Transform alternativeRotator;
    [SerializeField] LineRenderer laserRenderer;
    [SerializeField] Transform LaserStartPoint;

    public float lastAttackTime { get; private set; }
    UnitCondition unitCondition;
    Targeter targeter;
    Transform rotator;
    float attackRange;
    float attackSpeed;
    float attackDelay;
    string shootingSound;
    bool lastShotWasCanceled;
    bool isAttacking;
    Vector3 targetPosition;

    public override void OnStartServer()
    {
        unitCondition = GetComponent<UnitCondition>();
        targeter = GetComponent<Targeter>();
        rotator = alternativeRotator == null ? transform : alternativeRotator;
        Stats stats = GetComponent<StatsBehaviour>().stats;
        attackRange = stats.attackRange;
        attackSpeed = stats.attackSpeed;
        attackDelay = stats.attackDelay;
        shootingSound = stats.shootingSound;

        if (attackDelay >= 1 / attackSpeed)
            Debug.LogWarning($"{name} has an attackDelay that is greater than the duration between it's attacks. This makes no sense.");
    }

    [ServerCallback]
    private void Update()
    {
        if (targeter.target == null)
            return;
        if (unitCondition != null)
            if (unitCondition.isStunned)
                return;

        if (InRange(out Vector3 vectorToTarget))
        {
            float angleToTarget = Mathf.Abs(Vector3.Angle(rotator.forward, vectorToTarget));
            Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget);
            rotator.rotation = Quaternion.RotateTowards(rotator.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            if (angleToTarget > minAngleToTarget)
                return;

            if (Time.time > lastAttackTime + 1 / attackSpeed || lastShotWasCanceled)
            {
                lastAttackTime = Time.time;
                BeginAttack();
            }
        }
    }

    [Server]
    public bool InRange(out Vector3 vectorToTarget)
    {
        vectorToTarget = targeter.target.transform.position - rotator.position;
        vectorToTarget.y = 0;
        bool isWithinRange = vectorToTarget.sqrMagnitude < attackRange * attackRange;
        return isWithinRange;
    }


    [Server]
    void BeginAttack()
    {
        isAttacking = true;
        lastShotWasCanceled = false;
        targetPosition = targeter.target.aimTransform.position;
        if (laserRenderer != null)
            RpcDrawLaser(LaserStartPoint.position, targetPosition);
        CancelInvoke();
        Invoke(nameof(FinishAttack), attackDelay);
    }

    [Server]
    void FinishAttack()
    {
        isAttacking = false;
        lastShotWasCanceled = false;
        if (laserRenderer != null)
            RpcFinishLaser();
        LaunchProjectile(targetPosition);
    }
    
    [Server]
    void LaunchProjectile(Vector3 targetPosition)
    {
        Quaternion projectileRotation = Quaternion.LookRotation(targetPosition - projectileSpawnPoint.position);
        GameObject projectileInstance = Instantiate(projetilePrefab, projectileSpawnPoint.position, projectileRotation);
        NetworkServer.Spawn(projectileInstance, connectionToClient);
        Projectile projectileComponent = projectileInstance.GetComponent<Projectile>();
        projectileComponent.shooter = gameObject;
        projectileComponent.targetPosition = targetPosition;
        AudioManager.instance.RpcPlay(shootingSound);
    }


    [ClientRpc]
    void RpcDrawLaser(Vector3 startPosition, Vector3 endPosition)
    {
        laserRenderer.enabled = true;
        laserRenderer.SetPosition(0, startPosition);
        laserRenderer.SetPosition(1, endPosition);
    }

    [ClientRpc]
    void RpcFinishLaser()
    {
        laserRenderer.enabled = false;
        Vector3[] doubleStartPosition = new Vector3[2] { laserRenderer.GetPosition(0), laserRenderer.GetPosition(0) };
        laserRenderer.SetPositions(doubleStartPosition);
    }

    [Server]
    public void CancelShot()
    {
        if (laserRenderer != null)
            RpcFinishLaser();
        CancelInvoke();
        if (isAttacking && attackDelay > 0)
            lastShotWasCanceled = true;
        isAttacking = false;

    }

}