using UnityEngine;
using UnityEngine.AI;

public class EnemyShooter : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolPointTolerance = 0.5f;
    [SerializeField] private float waitAtPoint = 1f;

    [Header("Combat")]
    [SerializeField] private float attackDistance = 20f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Take Cover")]
    [SerializeField] private Transform[] coverPoints;
    [SerializeField] private float lowHealthThreshold = 30f;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private float coverPointTolerance = 0.8f;

    [Header("Predictive Shooting")]
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float aimSpread = 2f;

    [Header("Shooting")]
    [SerializeField] private LayerMask hitMask = ~0;

    private Enemy enemy;
    private NavMeshAgent agent;
    private Health health;

    private int currentPatrolIndex;

    private Vector3 lastKnownPlayerPosition;

    public Enemy Enemy => enemy;
    public NavMeshAgent Agent => agent;
    public Health Health => health;

    public Transform[] PatrolPoints => patrolPoints;
    public float PatrolPointTolerance => patrolPointTolerance;
    public float WaitAtPoint => waitAtPoint;

    public int CurrentPatrolIndex => currentPatrolIndex;

    public float AttackDistance => attackDistance;
    public float AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;

    public Transform[] CoverPoints => coverPoints;
    public float LowHealthThreshold => lowHealthThreshold;
    public float ReloadTime => reloadTime;
    public float CoverPointTolerance => coverPointTolerance;

    public float ProjectileSpeed => projectileSpeed;
    public float AimSpread => aimSpread;
    public LayerMask HitMask => hitMask;

    public Vector3 LastKnownPlayerPosition =>
        lastKnownPlayerPosition;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

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
    }

    public void SetPatrolIndex(int index)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        currentPatrolIndex =
            Mathf.Clamp(index, 0, patrolPoints.Length - 1);
    }

    public Transform GetCurrentPatrolPoint()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return null;
        }

        return patrolPoints[currentPatrolIndex];
    }

    public void MoveToCurrentPatrolPoint()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        Transform point = GetCurrentPatrolPoint();

        if (point == null)
            return;

        agent.isStopped = false;
        agent.SetDestination(point.position);
    }

    public bool HasReachedCurrentPatrolPoint()
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        if (agent.pathPending)
            return false;

        if (agent.remainingDistance >
            Mathf.Max(
                agent.stoppingDistance,
                patrolPointTolerance))
        {
            return false;
        }

        return true;
    }

    public void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
            currentPatrolIndex = 0;

        MoveToCurrentPatrolPoint();
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

    public void SetLastKnownPlayerPosition(Vector3 position)
    {
        lastKnownPlayerPosition = position;
    }

    public bool HasReachedPosition(Vector3 position)
    {
        if (agent == null ||
            !agent.isOnNavMesh)
        {
            return false;
        }

        if (agent.pathPending)
            return false;

        return agent.remainingDistance <=
               Mathf.Max(
                   agent.stoppingDistance,
                   patrolPointTolerance);
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

        return health.CurrentHealth <=
               lowHealthThreshold;
    }

    public Transform FindBestCoverPoint()
    {
        if (coverPoints == null ||
            coverPoints.Length == 0)
        {
            return null;
        }

        Transform bestPoint = null;
        float bestDistance = Mathf.Infinity;

        foreach (Transform point in coverPoints)
        {
            if (point == null)
                continue;

            NavMeshPath path = new NavMeshPath();

            if (!NavMesh.CalculatePath(
                    transform.position,
                    point.position,
                    NavMesh.AllAreas,
                    path))
            {
                continue;
            }

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    point.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestPoint = point;
            }
        }

        return bestPoint;
    }

    public void MoveToCover(Transform coverPoint)
    {
        if (agent == null ||
            !agent.isOnNavMesh ||
            coverPoint == null)
        {
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(coverPoint.position);
    }

    public bool HasReachedCover(Transform coverPoint)
    {
        if (coverPoint == null)
            return false;

        if (agent == null ||
            !agent.isOnNavMesh)
        {
            return false;
        }

        if (agent.pathPending)
            return false;

        return agent.remainingDistance <=
               Mathf.Max(
                   agent.stoppingDistance,
                   coverPointTolerance);
    }
}