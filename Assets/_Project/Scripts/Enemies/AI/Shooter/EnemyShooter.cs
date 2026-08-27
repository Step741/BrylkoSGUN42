using UnityEngine;
using UnityEngine.AI;

public class EnemyShooter : MonoBehaviour
{
    // ==========================================
    // PATROL
    // ==========================================

    [Header("Patrol")]

    [SerializeField]
    private Transform[] patrolPoints;

    [SerializeField]
    private float patrolPointTolerance = 0.5f;

    [SerializeField]
    private float waitAtPoint = 1f;


    // ==========================================
    // COMBAT
    // ==========================================

    [Header("Combat")]

    [SerializeField]
    private float attackDistance = 20f;

    [SerializeField]
    private float attackDamage = 15f;

    [SerializeField]
    private float attackCooldown = 1f;


    // ==========================================
    // TAKE COVER
    // ==========================================

    [Header("Take Cover")]

    [SerializeField]
    private Transform[] coverPoints;

    [SerializeField]
    private float lowHealthThreshold = 30f;

    [SerializeField]
    private float reloadTime = 2f;

    [SerializeField]
    private float coverPointTolerance = 0.8f;


    // ==========================================
    // PREDICTIVE SHOOTING
    // ==========================================

    [Header("Predictive Shooting")]

    [SerializeField]
    private float projectileSpeed = 20f;

    [SerializeField]
    private float aimSpread = 0f;


    // ==========================================
    // PROJECTILE
    // ==========================================

    [Header("Projectile")]

    [SerializeField]
    private SpitProjectilePool projectilePool;

    [SerializeField]
    private Transform spitOrigin;


    // ==========================================
    // MOVEMENT
    // ==========================================

    [Header("Movement")]

    [SerializeField]
    private float moveSpeed = 3.5f;

    [SerializeField]
    private float rotationSpeed = 8f;


    // ==========================================
    // ANIMATION
    // ==========================================

    [Header("Animation")]

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float animationSpeedDampTime = 0.1f;

    [SerializeField]
    private float movementAnimationThreshold = 0.05f;


    // ==========================================
    // ANIMATOR HASHES
    // ==========================================

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int SpitAttackHash =
        Animator.StringToHash("SpitAttack");

    private static readonly int DieHash =
        Animator.StringToHash("Die");


    // ==========================================
    // COMPONENTS
    // ==========================================

    private Enemy enemy;

    private NavMeshAgent agent;

    private Health health;


    // ==========================================
    // PATROL STATE
    // ==========================================

    private int currentPatrolIndex;


    // ==========================================
    // PLAYER MEMORY
    // ==========================================

    private Vector3 lastKnownPlayerPosition;


    // ==========================================
    // DISABLE DEATH COLLIDERS
    // ==========================================

    private void DisableDeathColliders()
    {
        Collider[] colliders =
            GetComponentsInChildren<Collider>(
                true
            );

        foreach (
            Collider collider
            in colliders
        )
        {
            if (collider == null)
                continue;

            collider.enabled =
                false;
        }
    }


    // ==========================================
    // DEATH
    // ==========================================

    private bool deathAnimationPlayed;


    // ==========================================
    // PENDING PROJECTILE
    // ==========================================

    private bool projectileQueued;

    private Vector3 queuedProjectileDirection;


    // ==========================================
    // PUBLIC PROPERTIES
    // ==========================================

    public Enemy Enemy =>
        enemy;

    public NavMeshAgent Agent =>
        agent;

    public Health Health =>
        health;

    public Transform[] PatrolPoints =>
        patrolPoints;

    public float PatrolPointTolerance =>
        patrolPointTolerance;

    public float WaitAtPoint =>
        waitAtPoint;

    public int CurrentPatrolIndex =>
        currentPatrolIndex;

    public float AttackDistance =>
        attackDistance;

    public float AttackDamage =>
        attackDamage;

    public float AttackCooldown =>
        attackCooldown;

    public Transform[] CoverPoints =>
        coverPoints;

    public float LowHealthThreshold =>
        lowHealthThreshold;

    public float ReloadTime =>
        reloadTime;

    public float CoverPointTolerance =>
        coverPointTolerance;

    public float ProjectileSpeed =>
        projectileSpeed;

    public Transform SpitOrigin =>
        spitOrigin != null
            ? spitOrigin
            : transform;

    public float AimSpread =>
        aimSpread;

    public Vector3 LastKnownPlayerPosition =>
        lastKnownPlayerPosition;


    // ==========================================
    // PROJECTILE POOL
    // ==========================================

    public void SetProjectilePool(
        SpitProjectilePool newPool
    )
    {
        projectilePool =
            newPool;
    }


    // ==========================================
    // UNITY
    // ==========================================

    private void Awake()
    {
        enemy =
            GetComponent<Enemy>();

        agent =
            GetComponent<NavMeshAgent>();

        health =
            GetComponent<Health>();


        // Применяем скорость из Inspector.
        if (agent != null)
        {
            agent.speed =
                moveSpeed;
        }


        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }


        if (spitOrigin == null)
        {
            spitOrigin =
                transform;
        }


        if (enemy == null)
        {
            Debug.LogError(
                $"[{name}] EnemyShooter requires Enemy component."
            );
        }


        if (agent == null)
        {
            Debug.LogError(
                $"[{name}] EnemyShooter requires NavMeshAgent."
            );
        }


        if (health == null)
        {
            Debug.LogError(
                $"[{name}] EnemyShooter requires Health component."
            );
        }


        if (animator == null)
        {
            Debug.LogError(
                $"[{name}] EnemyShooter requires Animator."
            );
        }


        if (health != null)
        {
            health.Died +=
                Die;
        }
    }


    private void Update()
    {
        UpdateMovementAnimation();
    }


    private void OnDestroy()
    {
        if (health != null)
        {
            health.Died -=
                Die;
        }
    }


    // ==========================================
    // MOVEMENT ANIMATION
    // ==========================================

    private void UpdateMovementAnimation()
    {
        if (animator == null)
            return;


        if (
            deathAnimationPlayed ||
            (health != null && health.IsDead)
        )
        {
            animator.SetFloat(
                SpeedHash,
                0f
            );

            return;
        }


        if (
            agent == null ||
            !agent.isOnNavMesh
        )
        {
            animator.SetFloat(
                SpeedHash,
                0f,
                animationSpeedDampTime,
                Time.deltaTime
            );

            return;
        }


        Vector3 velocity =
            agent.velocity;


        if (
            velocity.sqrMagnitude <
            movementAnimationThreshold *
            movementAnimationThreshold
        )
        {
            velocity =
                agent.desiredVelocity;
        }


        velocity.y =
            0f;


        if (
            velocity.sqrMagnitude <
            movementAnimationThreshold *
            movementAnimationThreshold
        )
        {
            animator.SetFloat(
                SpeedHash,
                0f,
                animationSpeedDampTime,
                Time.deltaTime
            );

            return;
        }


        Vector3 localVelocity =
            transform.InverseTransformDirection(
                velocity
            );


        float targetSpeed =
            localVelocity.z >= 0f
                ? 1f
                : -1f;


        animator.SetFloat(
            SpeedHash,
            targetSpeed,
            animationSpeedDampTime,
            Time.deltaTime
        );
    }


    // ==========================================
    // ATTACK ANIMATION
    // ==========================================

    public void StartAttackAnimation()
    {
        if (animator == null)
            return;

        if (deathAnimationPlayed)
            return;


        StopMoving();


        animator.SetFloat(
            SpeedHash,
            0f
        );


        animator.ResetTrigger(
            SpitAttackHash
        );

        animator.SetTrigger(
            SpitAttackHash
        );
    }


    // ==========================================
    // QUEUE PROJECTILE
    // ==========================================

    public void QueueProjectile(
        Vector3 direction
    )
    {
        if (
            direction.sqrMagnitude <=
            0.001f
        )
        {
            return;
        }


        queuedProjectileDirection =
            direction.normalized;

        projectileQueued =
            true;
    }


    // ==========================================
    // FIRE PROJECTILE
    // ==========================================

    public void FireQueuedProjectile()
    {
        if (!projectileQueued)
            return;


        if (deathAnimationPlayed)
        {
            projectileQueued =
                false;

            return;
        }


        if (projectilePool == null)
        {
            Debug.LogWarning(
                $"[{name}] Projectile pool is missing."
            );

            projectileQueued =
                false;

            return;
        }


        Transform origin =
            spitOrigin != null
                ? spitOrigin
                : transform;


        SpitProjectile projectile =
            projectilePool.GetProjectile(
                origin.position,
                Quaternion.LookRotation(
                    queuedProjectileDirection
                )
            );


        if (projectile == null)
        {
            Debug.LogWarning(
                $"[{name}] Could not get projectile from pool."
            );

            projectileQueued =
                false;

            return;
        }


        projectile.SetDamage(
            attackDamage
        );


        projectile.Launch(
            queuedProjectileDirection,
            projectileSpeed,
            attackDamage,
            transform
        );


        projectileQueued =
            false;
    }


    // ==========================================
    // DEATH
    // ==========================================

    private void Die()
    {
        if (deathAnimationPlayed)
            return;


        deathAnimationPlayed =
            true;

        ImpactDecal.RemoveDecalsForTarget(
            transform
        );


        // Отключаем все коллайдеры врага,
        // чтобы после смерти пули больше не попадали
        // в вертикальный коллайдер и не создавали декали в воздухе.
        DisableDeathColliders();


        projectileQueued =
            false;


        StopMoving();


        if (animator == null)
            return;


        animator.SetFloat(
            SpeedHash,
            0f
        );


        animator.ResetTrigger(
            SpitAttackHash
        );

        animator.ResetTrigger(
            DieHash
        );

        animator.SetTrigger(
            DieHash
        );
    }


    // ==========================================
    // PATROL
    // ==========================================

    public void SetPatrolIndex(
        int index
    )
    {
        if (
            patrolPoints == null ||
            patrolPoints.Length == 0
        )
        {
            return;
        }


        currentPatrolIndex =
            Mathf.Clamp(
                index,
                0,
                patrolPoints.Length - 1
            );
    }


    public Transform GetCurrentPatrolPoint()
    {
        if (
            patrolPoints == null ||
            patrolPoints.Length == 0
        )
        {
            return null;
        }


        return patrolPoints[
            currentPatrolIndex
        ];
    }


    public void MoveToCurrentPatrolPoint()
    {
        if (
            agent == null ||
            !agent.isOnNavMesh
        )
        {
            return;
        }


        Transform point =
            GetCurrentPatrolPoint();

        if (point == null)
            return;


        agent.isStopped =
            false;

        agent.SetDestination(
            point.position
        );
    }


    public bool HasReachedCurrentPatrolPoint()
    {
        if (
            agent == null ||
            !agent.isOnNavMesh
        )
        {
            return false;
        }


        if (agent.pathPending)
            return false;


        return
            agent.remainingDistance <=
            Mathf.Max(
                agent.stoppingDistance,
                patrolPointTolerance
            );
    }


    public void MoveToNextPatrolPoint()
    {
        if (
            patrolPoints == null ||
            patrolPoints.Length == 0
        )
        {
            return;
        }


        currentPatrolIndex++;


        if (
            currentPatrolIndex >=
            patrolPoints.Length
        )
        {
            currentPatrolIndex =
                0;
        }


        MoveToCurrentPatrolPoint();
    }


    // ==========================================
    // MOVEMENT
    // ==========================================

    public void StopMoving()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;


        agent.isStopped =
            true;

        agent.ResetPath();
    }


    public void LookAtPlayer(
        Transform player
    )
    {
        if (player == null)
            return;


        Vector3 direction =
            player.position -
            transform.position;

        direction.y =
            0f;


        if (
            direction.sqrMagnitude <
            0.001f
        )
        {
            return;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }


    // ==========================================
    // PLAYER MEMORY
    // ==========================================

    public void SetLastKnownPlayerPosition(
        Vector3 position
    )
    {
        lastKnownPlayerPosition =
            position;
    }


    public bool HasReachedPosition(
        Vector3 position
    )
    {
        if (
            agent == null ||
            !agent.isOnNavMesh
        )
        {
            return false;
        }


        if (agent.pathPending)
            return false;


        return
            agent.remainingDistance <=
            Mathf.Max(
                agent.stoppingDistance,
                patrolPointTolerance
            );
    }


    // ==========================================
    // TAKE COVER
    // ==========================================

    public bool NeedsToTakeCover()
    {
        if (health == null)
            return false;

        if (health.IsDead)
            return false;


        return
            health.CurrentHealth <=
            lowHealthThreshold;
    }


    public Transform FindBestCoverPoint()
    {
        if (
            coverPoints == null ||
            coverPoints.Length == 0
        )
        {
            return null;
        }


        Transform bestPoint =
            null;

        float bestDistance =
            Mathf.Infinity;

        NavMeshPath path =
            new NavMeshPath();


        foreach (
            Transform point
            in coverPoints
        )
        {
            if (point == null)
                continue;


            if (
                !NavMesh.CalculatePath(
                    transform.position,
                    point.position,
                    NavMesh.AllAreas,
                    path
                )
            )
            {
                continue;
            }


            if (
                path.status !=
                NavMeshPathStatus.PathComplete
            )
            {
                continue;
            }


            if (!IsCoverPointProtected(point))
                continue;


            float distance =
                Vector3.Distance(
                    transform.position,
                    point.position
                );


            if (distance < bestDistance)
            {
                bestDistance =
                    distance;

                bestPoint =
                    point;
            }
        }


        return bestPoint;
    }


    private bool IsCoverPointProtected(
        Transform coverPoint
    )
    {
        if (coverPoint == null)
            return false;

        if (
            Enemy == null ||
            Enemy.Vision == null
        )
        {
            return false;
        }


        Transform player =
            Enemy.Vision.Player;


        if (player == null)
            return false;


        Vector3 origin =
            coverPoint.position;

        Vector3 direction =
            player.position -
            origin;

        float distance =
            direction.magnitude;


        if (distance <= 0.01f)
            return false;


        direction.Normalize();


        return Physics.Raycast(
            origin,
            direction,
            distance,
            Enemy.Vision.ObstacleMask,
            QueryTriggerInteraction.Ignore
        );
    }


    public void MoveToCover(
        Transform coverPoint
    )
    {
        if (
            agent == null ||
            !agent.isOnNavMesh ||
            coverPoint == null
        )
        {
            return;
        }


        agent.isStopped =
            false;

        agent.SetDestination(
            coverPoint.position
        );
    }


    public bool HasReachedCover(
        Transform coverPoint
    )
    {
        if (
            agent == null ||
            !agent.isOnNavMesh ||
            coverPoint == null
        )
        {
            return false;
        }


        if (agent.pathPending)
            return false;


        return
            agent.remainingDistance <=
            Mathf.Max(
                agent.stoppingDistance,
                coverPointTolerance
            );
    }


    public void CancelAttackAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(
            SpitAttackHash
        );
    }


    public bool IsAttackAnimationPlaying()
    {
        if (animator == null)
            return false;

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(
                0
            );

        return
            stateInfo.IsTag(
                "Attack"
            ) ||
            stateInfo.IsName(
                "SpitAttack"
            );
    }


    // ==========================================
    // GIZMOS
    // ==========================================

    private void OnDrawGizmos()
    {
        DrawPatrolRoute();

        DrawCurrentStateTarget();
    }


    // ==========================================
    // PATROL ROUTE
    // ==========================================

    private void DrawPatrolRoute()
    {
        if (
            patrolPoints == null ||
            patrolPoints.Length == 0
        )
        {
            return;
        }


        Gizmos.color =
            new Color(
                0.2f,
                0.8f,
                0.2f,
                0.35f
            );


        for (
            int i = 0;
            i < patrolPoints.Length;
            i++
        )
        {
            Transform point =
                patrolPoints[i];


            if (point == null)
                continue;


            Gizmos.DrawWireSphere(
                point.position,
                0.3f
            );


            int nextIndex =
                (i + 1) %
                patrolPoints.Length;


            Transform nextPoint =
                patrolPoints[nextIndex];


            if (nextPoint == null)
                continue;


            Gizmos.DrawLine(
                point.position,
                nextPoint.position
            );
        }
    }


    // ==========================================
    // CURRENT STATE TARGET
    // ==========================================

    private void DrawCurrentStateTarget()
    {
        if (enemy == null)
        {
            enemy =
                GetComponent<Enemy>();
        }


        if (
            enemy == null ||
            enemy.StateMachine == null
        )
        {
            return;
        }


        EnemyState currentState =
            enemy.StateMachine.CurrentState;


        if (currentState == null)
            return;


        if (
            currentState is ShooterPatrolState
        )
        {
            Transform patrolTarget =
                GetCurrentPatrolPoint();


            if (patrolTarget == null)
                return;


            Gizmos.color =
                Color.green;


            DrawCurrentTarget(
                patrolTarget.position,
                0.55f
            );

            return;
        }


        if (
            currentState is ShooterSearchState
        )
        {
            Gizmos.color =
                Color.yellow;


            DrawCurrentTarget(
                lastKnownPlayerPosition,
                0.6f
            );

            return;
        }


        if (
            currentState is ShooterCombatState
        )
        {
            if (
                enemy.Vision == null ||
                enemy.Vision.Player == null
            )
            {
                return;
            }


            Gizmos.color =
                Color.red;


            DrawCurrentTarget(
                enemy.Vision.Player.position,
                0.7f
            );

            return;
        }


        if (
            currentState is ShooterBackstepState
        )
        {
            Gizmos.color =
                Color.cyan;


            DrawAgentDestination(
                0.55f
            );

            return;
        }


        if (
            currentState is ShooterTakeCoverState
        )
        {
            Gizmos.color =
                Color.magenta;


            DrawAgentDestination(
                0.65f
            );

            return;
        }


        Gizmos.color =
            Color.gray;


        if (
            agent != null &&
            agent.isOnNavMesh &&
            agent.hasPath
        )
        {
            DrawCurrentTarget(
                agent.destination,
                0.4f
            );
        }
    }


    // ==========================================
    // DRAW CURRENT TARGET
    // ==========================================

    private void DrawCurrentTarget(
        Vector3 targetPosition,
        float radius
    )
    {
        Vector3 startPosition =
            transform.position +
            Vector3.up *
            0.5f;


        Gizmos.DrawLine(
            startPosition,
            targetPosition
        );


        Gizmos.DrawWireSphere(
            targetPosition,
            radius
        );


        Gizmos.DrawSphere(
            targetPosition,
            radius * 0.12f
        );
    }


    // ==========================================
    // DRAW NAVMESH DESTINATION
    // ==========================================

    private void DrawAgentDestination(
        float radius
    )
    {
        if (
            agent == null ||
            !agent.isOnNavMesh ||
            !agent.hasPath
        )
        {
            return;
        }


        DrawCurrentTarget(
            agent.destination,
            radius
        );
    }
}