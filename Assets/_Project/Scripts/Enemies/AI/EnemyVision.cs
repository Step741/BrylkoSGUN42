using UnityEngine;
using Zenject;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision")]

    [SerializeField]
    private float viewRadius = 15f;


    [SerializeField]
    [Range(0f, 180f)]
    private float viewAngle = 60f;


    [SerializeField]
    private float eyeHeight = 1.5f;


    [SerializeField]
    private float playerTargetHeight = 1f;

    [Header("Line Of Sight")]

    [SerializeField]
    private LayerMask obstacleMask;

    [Header("Gizmos")]

    [SerializeField]
    private bool showGizmos = true;

    private Transform player;

    [Inject]
    private void Construct(
        PlayerTarget playerTarget
    )
    {
        player =
            playerTarget.Transform;
    }

    public bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 enemyPosition =
            transform.position;


        Vector3 playerPosition =
            player.position;

        Vector3 directionToPlayer =
            playerPosition -
            enemyPosition;


        float distanceToPlayer =
            directionToPlayer.magnitude;


        if (distanceToPlayer > viewRadius)
            return false;

        Vector3 flatDirection =
            directionToPlayer;


        flatDirection.y =
            0f;


        if (flatDirection.sqrMagnitude <= 0.001f)
            return true;


        flatDirection.Normalize();


        float angleToPlayer =
            Vector3.Angle(
                transform.forward,
                flatDirection
            );


        if (
            angleToPlayer >
            viewAngle * 0.5f
        )
        {
            return false;
        }

        Vector3 origin =
            transform.position +
            Vector3.up *
            eyeHeight;


        Vector3 target =
            player.position +
            Vector3.up *
            playerTargetHeight;


        Vector3 direction =
            target -
            origin;


        float distance =
            direction.magnitude;


        if (distance <= 0.01f)
            return true;


        direction.Normalize();

        if (
            Physics.Raycast(
                origin,
                direction,
                distance,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            return false;
        }
        return true;
    }

    //GIZMOS
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos)
            return;

        Gizmos.color =
            Color.yellow;


        Gizmos.DrawWireSphere(
            transform.position,
            viewRadius
        );

        Gizmos.color =
            Color.blue;


        Gizmos.DrawRay(
            transform.position,
            transform.forward *
            viewRadius
        );

        Vector3 leftBoundary =
            DirectionFromAngle(
                -viewAngle * 0.5f
            );

        Vector3 rightBoundary =
            DirectionFromAngle(
                viewAngle * 0.5f
            );


        Gizmos.color =
            Color.green;


        Gizmos.DrawRay(
            transform.position,
            leftBoundary *
            viewRadius
        );


        Gizmos.DrawRay(
            transform.position,
            rightBoundary *
            viewRadius
        );

        if (player != null)
        {
            Vector3 origin =
                transform.position +
                Vector3.up *
                eyeHeight;


            Vector3 target =
                player.position +
                Vector3.up *
                playerTargetHeight;


            Vector3 direction =
                target -
                origin;


            float distance =
                direction.magnitude;


            if (distance > 0.01f)
            {
                bool blocked =
                    Physics.Raycast(
                        origin,
                        direction.normalized,
                        distance,
                        obstacleMask,
                        QueryTriggerInteraction.Ignore
                    );


                Gizmos.color =
                    blocked
                        ? Color.red
                        : Color.green;


                Gizmos.DrawLine(
                    origin,
                    target
                );
            }
        }
    }

    private Vector3 DirectionFromAngle(
        float angle
    )
    {
        Quaternion rotation =
            Quaternion.Euler(
                0f,
                angle,
                0f
            );


        return
            rotation *
            transform.forward;
    }

    public float ViewRadius =>
        viewRadius;


    public float ViewAngle =>
        viewAngle;


    public Transform Player =>
        player;


    public LayerMask ObstacleMask =>
        obstacleMask;
}