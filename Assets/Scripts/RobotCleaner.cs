using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RobotCleaner : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 3f;

    [SerializeField]
    private float turnSpeed = 180f;

    [SerializeField]
    private float reverseSpeed = 2f;

    [SerializeField]
    private float reverseTime = 0.5f;

    [Header("Raycasts")]
    [SerializeField]
    private float rayDistance = 1.7f;

    [SerializeField]
    private float sideRayAngle = 35f;

    [SerializeField]
    private LayerMask obstacleLayer;

    [Header("Random Movement")]
    [SerializeField]
    private float randomTurnInterval = 5f;

    [SerializeField]
    private float randomTurnAngle = 90f;

    [Header("Garbage")]
    [SerializeField]
    private float collectRadius = 1.2f;

    [SerializeField]
    private LayerMask garbageLayer;

    [Header("Audio")]
    [SerializeField]
    private AudioSource engineAudio;

    [SerializeField]
    private AudioSource collisionAudio;

    private Rigidbody rb;

    private float randomTimer;
    private float currentTurn;

    private bool isTurning;
    private bool isReversing;
    private float reverseTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        randomTimer = randomTurnInterval;
        if (engineAudio != null)
        {
            engineAudio.Play();
        }
    }

    private void FixedUpdate()
    {
        Move();

        DetectObstacles();

        RandomMovement();

        CollectGarbage();
    }

    private void Move()
    {
        if (isReversing)
        {
            rb.MovePosition(
                rb.position -
                transform.forward *
                reverseSpeed *
                Time.fixedDeltaTime);

            reverseTimer -= Time.fixedDeltaTime;

            if (reverseTimer <= 0f)
            {
                isReversing = false;

                float angle = Random.Range(110f, 170f);

                if (Random.value < 0.5f)
                    angle = -angle;

                Turn(angle);
            }

            return;
        }

        if (isTurning)
        {
            float rotation =
                Mathf.Sign(currentTurn) *
                turnSpeed *
                Time.fixedDeltaTime;

            if (Mathf.Abs(rotation) > Mathf.Abs(currentTurn))
                rotation = currentTurn;

            rb.MoveRotation(
                rb.rotation *
                Quaternion.Euler(0f, rotation, 0f));

            currentTurn -= rotation;

            if (Mathf.Abs(currentTurn) < 0.5f)
            {
                currentTurn = 0f;
                isTurning = false;
            }

            return;
        }

        rb.MovePosition(
            rb.position +
            transform.forward *
            moveSpeed *
            Time.fixedDeltaTime);
    }

    private void DetectObstacles()
    {
        if (isTurning)
            return;

        Vector3 origin = transform.position + Vector3.up * 0.3f;

        Vector3 leftDirection =
            Quaternion.Euler(0f, -sideRayAngle, 0f) * transform.forward;

        Vector3 rightDirection =
            Quaternion.Euler(0f, sideRayAngle, 0f) * transform.forward;

        bool frontBlocked = Physics.Raycast(
            origin,
            transform.forward,
            rayDistance,
            obstacleLayer);

        bool leftBlocked = Physics.Raycast(
            origin,
            leftDirection,
            rayDistance,
            obstacleLayer);

        bool rightBlocked = Physics.Raycast(
            origin,
            rightDirection,
            rayDistance,
            obstacleLayer);

        if (!frontBlocked && !leftBlocked && !rightBlocked)
            return;

        if (collisionAudio != null)
        {
            collisionAudio.PlayOneShot(collisionAudio.clip);
        }

        if (!leftBlocked && !rightBlocked)
        {
            Turn(Random.value < 0.5f ? -90f : 90f);
        }
        else if (!leftBlocked)
        {
            Turn(-90f);
        }
        else if (!rightBlocked)
        {
            Turn(90f);
        }
        else
        {
            StartReverse();
        }
    }

    private void RandomMovement()
    {
        if (isTurning)
            return;

        randomTimer -= Time.fixedDeltaTime;

        if (randomTimer > 0f)
            return;

        randomTimer = randomTurnInterval;

        Turn(Random.value < 0.5f ? randomTurnAngle : -randomTurnAngle);
    }

    private void CollectGarbage()
    {
        Collider[] garbageObjects = Physics.OverlapSphere(
            transform.position,
            collectRadius,
            garbageLayer);

        foreach (Collider item in garbageObjects)
        {
            Garbage garbage = item.GetComponent<Garbage>();

            if (garbage != null)
            {
                garbage.Collect();
            }
        }
    }

    private void Turn(float angle)
    {
        if (isTurning)
            return;

        currentTurn = angle;
        isTurning = true;
    }

    private void StartReverse()
    {
        if (isTurning || isReversing)
            return;

        reverseTimer = reverseTime;
        isReversing = true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * 0.3f;

        Vector3 leftDirection =
            Quaternion.Euler(0f, -sideRayAngle, 0f) * transform.forward;

        Vector3 rightDirection =
            Quaternion.Euler(0f, sideRayAngle, 0f) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, transform.forward * rayDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, leftDirection * rayDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(origin, rightDirection * rayDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}