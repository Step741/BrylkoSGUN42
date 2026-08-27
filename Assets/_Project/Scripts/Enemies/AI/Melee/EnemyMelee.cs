using UnityEngine;
using UnityEngine.AI;

public class EnemyMelee :
    MonoBehaviour,
    IStunnable
{
    [Header("Combat")]

    [SerializeField]
    private float attackDistance = 2f;

    [SerializeField]
    private float attackDamage = 20f;

    [SerializeField]
    private float attackCooldown = 1.2f;


    [Header("Melee Hit")]

    [SerializeField]
    private LayerMask attackTargetMask;

    [SerializeField]
    private float attackSectorAngle = 120f;


    [Header("Movement")]

    [SerializeField]
    private float moveSpeed = 3f;

    [SerializeField]
    private float chaseStoppingDistance = 1.5f;

    [SerializeField]
    private float rotationSpeed = 8f;


    [Header("Animation")]

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float movementAnimationThreshold = 0.05f;


    [Header("Stunned")]

    [SerializeField]
    private float stunDuration = 1.5f;


    // ==========================================
    // ANIMATOR HASHES
    // ==========================================

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int ClawAttackHash =
        Animator.StringToHash("ClawAttack");

    private static readonly int DieHash =
        Animator.StringToHash("Die");


    // ==========================================
    // COMPONENTS
    // ==========================================

    private Enemy enemy;
    private NavMeshAgent agent;
    private Health health;

    private EnemySoundController
        enemySoundController;


    // ==========================================
    // STATE
    // ==========================================

    private bool attackAnimationPlaying;
    private bool deathAnimationPlayed;


    // ==========================================
    // ATTACK
    // ==========================================

    private readonly Collider[] attackResults =
        new Collider[16];


    // ==========================================
    // PUBLIC PROPERTIES
    // ==========================================

    public Enemy Enemy => enemy;

    public NavMeshAgent Agent => agent;

    public Health Health => health;

    public float AttackDistance =>
        attackDistance;

    public float AttackDamage =>
        attackDamage;

    public float AttackCooldown =>
        attackCooldown;

    public float ChaseStoppingDistance =>
        chaseStoppingDistance;

    public float StunDuration =>
        stunDuration;


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

        enemySoundController =
            GetComponent<EnemySoundController>();


        if (agent != null)
        {
            agent.speed = moveSpeed;
        }


        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }


        if (enemy == null)
        {
            Debug.LogError(
                $"[{name}] EnemyMelee requires Enemy component."
            );
        }


        if (agent == null)
        {
            Debug.LogError(
                $"[{name}] EnemyMelee requires NavMeshAgent."
            );
        }


        if (health == null)
        {
            Debug.LogError(
                $"[{name}] EnemyMelee requires Health component."
            );
        }


        if (animator == null)
        {
            Debug.LogError(
                $"[{name}] EnemyMelee requires Animator."
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


        // После смерти всегда остаёмся на Death.
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


        // Если NavMeshAgent остановлен — Idle.
        if (
            agent == null ||
            !agent.isOnNavMesh ||
            agent.isStopped
        )
        {
            animator.SetFloat(
                SpeedHash,
                0f
            );

            return;
        }


        // Берём фактическое направление движения.
        Vector3 velocity =
            agent.velocity;

        velocity.y =
            0f;


        // Если агент ещё не успел набрать скорость,
        // используем направление, куда он хочет двигаться.
        if (
            velocity.sqrMagnitude <
            movementAnimationThreshold *
            movementAnimationThreshold
        )
        {
            velocity =
                agent.desiredVelocity;

            velocity.y =
                0f;
        }


        // Если движения действительно нет — Idle.
        if (
            velocity.sqrMagnitude <
            movementAnimationThreshold *
            movementAnimationThreshold
        )
        {
            animator.SetFloat(
                SpeedHash,
                0f
            );

            return;
        }


        // Определяем направление движения
        // относительно направления врага.
        float direction =
            Vector3.Dot(
                transform.forward,
                velocity.normalized
            );


        // Даём Animator только 3 стабильных значения:
        //
        //  1  = WalkForward
        //  0  = Idle
        // -1  = WalkBackward

        if (direction > 0.1f)
        {
            animator.SetFloat(
                SpeedHash,
                1f
            );
        }
        else if (direction < -0.1f)
        {
            animator.SetFloat(
                SpeedHash,
                -1f
            );
        }
        else
        {
            // Если враг движется вбок,
            // считаем это движением вперёд.
            animator.SetFloat(
                SpeedHash,
                1f
            );
        }
    }


    // ==========================================
    // MOVEMENT
    // ==========================================

    public void MoveToPlayer(
        Transform player)
    {
        if (player == null)
            return;


        if (
            agent == null ||
            !agent.isOnNavMesh
        )
        {
            return;
        }


        agent.isStopped =
            false;


        agent.stoppingDistance =
            chaseStoppingDistance;


        agent.SetDestination(
            player.position
        );


        LookAtPlayer(
            player
        );
    }


    public void StopMoving()
    {
        if (agent == null)
            return;


        if (!agent.isOnNavMesh)
            return;


        agent.isStopped =
            true;


        agent.ResetPath();


        if (animator != null)
        {
            animator.SetFloat(
                SpeedHash,
                0f
            );
        }
    }


    public void LookAtPlayer(
        Transform player)
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
    // ATTACK RANGE
    // ==========================================

    public bool IsPlayerInAttackRange(
        Transform player)
    {
        if (player == null)
            return false;


        Vector3 direction =
            player.position -
            transform.position;


        direction.y =
            0f;


        return direction.magnitude <=
               attackDistance;
    }


    // ==========================================
    // ATTACK ANIMATION
    // ==========================================

    public void StartAttackAnimation()
    {
        if (animator == null)
            return;


        if (attackAnimationPlaying)
            return;


        if (deathAnimationPlayed)
            return;


        attackAnimationPlaying =
            true;


        animator.SetFloat(
            SpeedHash,
            0f
        );


        animator.ResetTrigger(
            ClawAttackHash
        );


        animator.SetTrigger(
            ClawAttackHash
        );
    }


    public bool IsAttackAnimationPlaying()
    {
        return attackAnimationPlaying;
    }


    // ==========================================
    // MELEE ATTACK
    // ==========================================

    /// <summary>
    /// Animation Event.
    /// Вызывается в момент фактического удара.
    /// </summary>
    public void PerformAttack()
    {
        if (!attackAnimationPlaying)
            return;


        if (enemySoundController != null)
        {
            enemySoundController.PlayAttack();
        }


        Vector3 attackCenter =
            transform.position;


        int hitCount =
            Physics.OverlapSphereNonAlloc(
                attackCenter,
                attackDistance,
                attackResults,
                attackTargetMask,
                QueryTriggerInteraction.Ignore
            );


        if (hitCount <= 0)
            return;


        float minDot =
            Mathf.Cos(
                attackSectorAngle *
                0.5f *
                Mathf.Deg2Rad
            );


        for (
            int i = 0;
            i < hitCount;
            i++
        )
        {
            Collider targetCollider =
                attackResults[i];


            if (targetCollider == null)
                continue;


            IDamageable damageable =
                targetCollider.GetComponentInParent<
                    IDamageable
                >();


            if (damageable == null)
                continue;


            Vector3 targetPoint =
                targetCollider.ClosestPoint(
                    attackCenter
                );


            Vector3 direction =
                targetPoint -
                attackCenter;


            direction.y =
                0f;


            if (
                direction.sqrMagnitude <
                0.001f
            )
            {
                continue;
            }


            direction.Normalize();


            float dot =
                Vector3.Dot(
                    transform.forward,
                    direction
                );


            if (dot < minDot)
                continue;


            if (enemySoundController != null)
            {
                enemySoundController.PlayClawHit();
            }


            damageable.TakeDamage(
                attackDamage
            );


            Debug.Log(
                $"[{name}] Melee attack hit " +
                $"{targetCollider.name} for " +
                $"{attackDamage} damage."
            );


            break;
        }
    }


    /// <summary>
    /// Animation Event.
    /// Вызывается в последнем кадре ClawAttack.
    /// </summary>
    public void FinishAttackAnimation()
    {
        if (!attackAnimationPlaying)
            return;


        attackAnimationPlaying =
            false;


        if (
            enemy == null ||
            enemy.StateMachine == null ||
            enemy.IsDead
        )
        {
            return;
        }


        enemy.StateMachine.ChangeState(
            new MeleeBackstepState(
                enemy
            )
        );
    }


    public void CancelAttack()
    {
        attackAnimationPlaying =
            false;


        if (animator == null)
            return;


        animator.ResetTrigger(
            ClawAttackHash
        );
    }


    // ==========================================
    // STUN
    // ==========================================

    public void Stun()
    {
        if (
            enemy == null ||
            enemy.StateMachine == null
        )
        {
            return;
        }


        if (
            health != null &&
            health.IsDead
        )
        {
            return;
        }


        enemy.StateMachine.ChangeState(
            new MeleeStunnedState(
                enemy
            )
        );
    }


    public void PlayStunnedAnimation()
    {
        if (animator == null)
            return;


        animator.SetFloat(
            SpeedHash,
            0f
        );
    }


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

    public void Die()
    {
        if (deathAnimationPlayed)
            return;


        deathAnimationPlayed =
            true;

        ImpactDecal.RemoveDecalsForTarget(
            transform
        );


        // Отключаем все коллайдеры врага,
        // чтобы после смерти новые попадания
        // и декали больше не регистрировались.
        DisableDeathColliders();


        CancelAttack();


        StopMoving();


        if (animator != null)
        {
            animator.SetFloat(
                SpeedHash,
                0f
            );


            animator.ResetTrigger(
                DieHash
            );


            animator.SetTrigger(
                DieHash
            );
        }


        if (
            enemy == null ||
            enemy.StateMachine == null
        )
        {
            return;
        }


        enemy.StateMachine.ChangeState(
            new MeleeDeadState(
                enemy
            )
        );
    }


    // ==========================================
    // GIZMOS
    // ==========================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            attackDistance
        );


        Vector3 forward =
            transform.forward;


        float halfAngle =
            attackSectorAngle *
            0.5f;


        Vector3 leftDirection =
            Quaternion.Euler(
                0f,
                -halfAngle,
                0f
            ) *
            forward;


        Vector3 rightDirection =
            Quaternion.Euler(
                0f,
                halfAngle,
                0f
            ) *
            forward;


        Gizmos.DrawLine(
            transform.position,
            transform.position +
            leftDirection *
            attackDistance
        );


        Gizmos.DrawLine(
            transform.position,
            transform.position +
            rightDirection *
            attackDistance
        );
    }
    // ==========================================
    // GIZMOS
    // ==========================================

    private void OnDrawGizmos()
    {
        DrawCurrentStateTarget();
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


        // ==========================================
        // IDLE
        // ==========================================

        if (
            currentState is MeleeIdleState
        )
        {
            Gizmos.color =
                Color.gray;

            return;
        }


        // ==========================================
        // CHASE
        // ==========================================

        if (
            currentState is MeleeChaseState
        )
        {
            Transform player =
                enemy.Vision != null
                    ? enemy.Vision.Player
                    : null;


            if (player == null)
                return;


            Gizmos.color =
                Color.yellow;


            DrawCurrentTarget(
                player.position,
                0.6f
            );

            return;
        }


        // ==========================================
        // ATTACK
        // ==========================================

        if (
            currentState is MeleeAttackState
        )
        {
            Transform player =
                enemy.Vision != null
                    ? enemy.Vision.Player
                    : null;


            if (player == null)
                return;


            Gizmos.color =
                Color.red;


            DrawCurrentTarget(
                player.position,
                0.75f
            );

            return;
        }


        // ==========================================
        // BACKSTEP
        // ==========================================

        if (
            currentState is MeleeBackstepState
        )
        {
            Gizmos.color =
                Color.cyan;


            DrawAgentDestination(
                0.55f
            );

            return;
        }


        // ==========================================
        // STUNNED
        // ==========================================

        if (
            currentState is MeleeStunnedState
        )
        {
            Gizmos.color =
                Color.blue;


            DrawStunnedMarker();

            return;
        }


        // ==========================================
        // DEFAULT
        // ==========================================

        Gizmos.color =
            Color.white;


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


        // ------------------------------------------
        // LINE TO TARGET
        // ------------------------------------------

        Gizmos.DrawLine(
            startPosition,
            targetPosition
        );


        // ------------------------------------------
        // TARGET MARKER
        // ------------------------------------------

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
    // DRAW AGENT DESTINATION
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


    // ==========================================
    // STUNNED MARKER
    // ==========================================

    private void DrawStunnedMarker()
    {
        Vector3 position =
            transform.position +
            Vector3.up *
            1.5f;


        Gizmos.DrawWireSphere(
            position,
            0.5f
        );


        Gizmos.DrawSphere(
            position,
            0.08f
        );
    }
}