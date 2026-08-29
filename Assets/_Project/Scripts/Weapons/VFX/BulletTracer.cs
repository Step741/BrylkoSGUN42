using System;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class BulletTracer : MonoBehaviour
{
    [Header("Tracer Settings")]

    [SerializeField]
    private float speed = 350f;

    [SerializeField]
    private float tailLength = 1.5f;

    private LineRenderer lineRenderer;

    private Vector3 startPoint;

    private Vector3 endPoint;

    private Vector3 direction;

    private float distance;

    private float traveledDistance;

    private bool isPlaying;

    public event Action<BulletTracer> Finished;
    private void Awake()
    {
        lineRenderer =
            GetComponent<LineRenderer>();


        lineRenderer.positionCount =
            2;

        lineRenderer.useWorldSpace =
            true;
    }


    private void OnDisable()
    {
        isPlaying =
            false;
    }

    public void Play(
        Vector3 start,
        Vector3 end)
    {
        startPoint =
            start;

        endPoint =
            end;


        Vector3 offset =
            endPoint -
            startPoint;


        distance =
            offset.magnitude;


        if (distance <= 0.001f)
        {
            Finish();

            return;
        }


        direction =
            offset /
            distance;


        traveledDistance =
            0f;


        isPlaying =
            true;


        lineRenderer.positionCount =
            2;


        lineRenderer.SetPosition(
            0,
            startPoint
        );

        lineRenderer.SetPosition(
            1,
            startPoint
        );
    }

    private void Update()
    {
        if (!isPlaying)
            return;


        traveledDistance +=
            speed *
            Time.deltaTime;


        float headDistance =
            Mathf.Min(
                traveledDistance,
                distance
            );


        Vector3 headPosition =
            startPoint +
            direction *
            headDistance;


        float tailDistance =
            Mathf.Max(
                0f,
                headDistance -
                tailLength
            );


        Vector3 tailPosition =
            startPoint +
            direction *
            tailDistance;


        lineRenderer.SetPosition(
            0,
            tailPosition
        );

        lineRenderer.SetPosition(
            1,
            headPosition
        );


        if (
            traveledDistance >=
            distance
        )
        {
            Finish();
        }
    }

    private void Finish()
    {
        if (!isPlaying)
            return;


        isPlaying =
            false;


        Finished?.Invoke(
            this
        );
    }
}