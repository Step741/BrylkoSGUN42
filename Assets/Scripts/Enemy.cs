using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] 
    private Transform playerTransform;

    [Header("View Settings")]
    [SerializeField] 
    private float viewRadius = 8f;

    [SerializeField]
    [Range(0f, 360f)]
    private float viewAngle = 120f;

    [SerializeField] 
    private Vector3 viewCenterOffset = Vector3.zero;

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        CalculateViewArea();
    }

    private void CalculateViewArea()
    {
        if (playerTransform == null)
            return;

        Vector3 viewCenter = ViewCenter;

        float distanceToPlayer = Vector3.Distance(viewCenter, playerTransform.position);

        if (distanceToPlayer > viewRadius)
            return;

        Vector3 directionToPlayer = (playerTransform.position - viewCenter).normalized;

        if (!IsInViewAngle(directionToPlayer))
            return;

        if (Physics.Raycast(viewCenter, directionToPlayer, out RaycastHit hit, viewRadius))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log($"{gameObject.name} видит игрока!");
            }
        }
    }

    private bool IsInViewAngle(Vector3 targetDirection)
    {
        float angle = Vector3.Angle(transform.forward, targetDirection);
        return angle <= viewAngle * 0.5f;
    }

    public float ViewRadius => viewRadius;

    public float ViewAngle => viewAngle;

    public Vector3 ViewCenter => transform.position + viewCenterOffset;
}
