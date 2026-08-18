using UnityEngine;
using Zenject;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField] private float viewRadius = 15f;
    [SerializeField][Range(0f, 180f)] private float viewAngle = 60f;

    [Header("Line Of Sight")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Gizmos")]
    [SerializeField] private bool showGizmos = true;

    private Transform player;

    [Inject]
    private void Construct(PlayerTarget playerTarget)
    {
        player = playerTarget.Transform;
    }

    public bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 directionToPlayer = player.position - transform.position;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewRadius)
            return false;

        directionToPlayer.Normalize();

        float angleToPlayer =
            Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer > viewAngle * 0.5f)
            return false;

        Vector3 origin = transform.position;

        if (Physics.Raycast(
                origin,
                directionToPlayer,
                out RaycastHit hit,
                distanceToPlayer,
                obstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player ||
                   hit.transform.IsChildOf(player);
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos)
            return;

        // Радиус зрения.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Центральное направление.
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(
            transform.position,
            transform.forward * viewRadius
        );

        // Левая граница конуса.
        Vector3 leftBoundary =
            DirectionFromAngle(-viewAngle * 0.5f);

        // Правая граница конуса.
        Vector3 rightBoundary =
            DirectionFromAngle(viewAngle * 0.5f);

        Gizmos.color = Color.green;

        Gizmos.DrawRay(
            transform.position,
            leftBoundary * viewRadius
        );

        Gizmos.DrawRay(
            transform.position,
            rightBoundary * viewRadius
        );
    }

    private Vector3 DirectionFromAngle(float angle)
    {
        Quaternion rotation =
            Quaternion.Euler(0f, angle, 0f);

        return rotation * transform.forward;
    }

    public float ViewRadius => viewRadius;
    public float ViewAngle => viewAngle;
    public Transform Player => player;
    public LayerMask ObstacleMask => obstacleMask;
}