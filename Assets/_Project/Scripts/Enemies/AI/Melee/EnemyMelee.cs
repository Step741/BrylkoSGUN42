using UnityEngine;
using UnityEngine.AI;

public class EnemyMelee :
    MonoBehaviour,
    IStunnable
{
    [Header("Combat")]
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Melee Hit")]
    [SerializeField] private LayerMask attackTargetMask;
    [SerializeField] private float attackSectorAngle = 120f;

    [Header("Movement")]
    [SerializeField] private float chaseStoppingDistance = 1.5f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Stunned")]
    [SerializeField] private float stunDuration = 1.5f;

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int StunnedHash =
        Animator.StringToHash("Stunned");

    private Enemy enemy;
    private NavMeshAgent agent;
    private Health health;

    private bool attackAnimationPlaying;

    private readonly Collider[] attackResults =
        new Collider[16];

    public Enemy Enemy => enemy;
    public NavMeshAgent Agent => agent;
    public Health Health => health;

    public float AttackDistance => attackDistance;
    public float AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;

    public float ChaseStoppingDistance =>
        chaseStoppingDistance;

    public float StunDuration => stunDuration;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

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
            health.Died += Die;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.Died -= Die;
        }
    }

    // ==========================================
    // MOVEMENT
    // ==========================================

    public void MoveToPlayer(Transform player)
    {
        if (player == null)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = false;

        agent.stoppingDistance =
            chaseStoppingDistance;

        agent.SetDestination(
            player.position
        );

        LookAtPlayer(player);
    }

    public void StopMoving()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    public void LookAtPlayer(Transform player)
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

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

        direction.y = 0f;

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

        attackAnimationPlaying = true;

        animator.ResetTrigger(
            AttackHash
        );

        animator.SetTrigger(
            AttackHash
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
    ///
    /// По ТЗ:
    /// OverlapSphereNonAlloc + Vector3.Dot.
    /// </summary>
    public void PerformAttack()
    {
        if (!attackAnimationPlaying)
            return;

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

        for (int i = 0; i < hitCount; i++)
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

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                continue;

            direction.Normalize();

            float dot =
                Vector3.Dot(
                    transform.forward,
                    direction
                );

            // Цель должна находиться
            // в переднем секторе удара.
            if (dot < minDot)
                continue;

            damageable.TakeDamage(
                attackDamage
            );

            Debug.Log(
                $"[{name}] Melee attack hit " +
                $"{targetCollider.name} for " +
                $"{attackDamage} damage."
            );

            // Один удар — один урон одной цели.
            break;
        }
    }

    /// <summary>
    /// Animation Event.
    /// Вызывается в последнем кадре Attack.
    /// </summary>
    public void FinishAttackAnimation()
    {
        if (!attackAnimationPlaying)
            return;

        attackAnimationPlaying = false;

        if (enemy == null ||
            enemy.StateMachine == null ||
            enemy.IsDead)
        {
            return;
        }

        enemy.StateMachine.ChangeState(
            new MeleeBackstepState(enemy)
        );
    }

    public void CancelAttack()
    {
        attackAnimationPlaying = false;

        if (animator == null)
            return;

        animator.ResetTrigger(
            AttackHash
        );
    }

    // ==========================================
    // STUN
    // ==========================================

    public void Stun()
    {
        if (enemy == null ||
            enemy.StateMachine == null)
        {
            return;
        }

        if (health != null &&
            health.IsDead)
        {
            return;
        }

        enemy.StateMachine.ChangeState(
            new MeleeStunnedState(enemy)
        );
    }

    public void PlayStunnedAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(
            StunnedHash
        );

        animator.SetTrigger(
            StunnedHash
        );
    }

    // ==========================================
    // DEATH
    // ==========================================

    public void Die()
    {
        if (enemy == null ||
            enemy.StateMachine == null)
        {
            return;
        }

        // Health уже выставил IsDead = true
        // перед вызовом события Died.
        // Поэтому здесь НЕ проверяем enemy.IsDead.

        enemy.StateMachine.ChangeState(
            new MeleeDeadState(enemy)
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
            attackSectorAngle * 0.5f;

        Vector3 leftDirection =
            Quaternion.Euler(
                0f,
                -halfAngle,
                0f
            ) * forward;

        Vector3 rightDirection =
            Quaternion.Euler(
                0f,
                halfAngle,
                0f
            ) * forward;

        Gizmos.DrawLine(
            transform.position,
            transform.position +
            leftDirection * attackDistance
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position +
            rightDirection * attackDistance
        );
    }
}