using System.Collections.Generic;
using UnityEngine;


public class BulletTracerPool : MonoBehaviour
{
    public static BulletTracerPool Instance
    {
        get;
        private set;
    }


    // =========================================================
    // POOL
    // =========================================================

    private class Pool
    {
        public readonly Queue<BulletTracer>
            Available =
                new Queue<BulletTracer>();
    }


    private readonly Dictionary<int, Pool>
        pools =
            new Dictionary<int, Pool>();


    private readonly Dictionary<BulletTracer, Pool>
        tracerPools =
            new Dictionary<BulletTracer, Pool>();


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(
                gameObject
            );

            return;
        }


        Instance =
            this;
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance =
                null;
        }
    }


    // =========================================================
    // PLAY
    // =========================================================

    public void Play(
        BulletTracer template,
        Vector3 start,
        Vector3 end)
    {
        if (template == null)
            return;


        int poolId =
            template.GetInstanceID();


        if (
            !pools.TryGetValue(
                poolId,
                out Pool pool
            )
        )
        {
            pool =
                new Pool();


            pools.Add(
                poolId,
                pool
            );
        }


        BulletTracer tracer =
            GetTracer(
                template,
                pool
            );


        tracer.transform.SetPositionAndRotation(
            start,
            Quaternion.identity
        );


        tracer.gameObject.SetActive(
            true
        );


        tracer.Play(
            start,
            end
        );
    }


    // =========================================================
    // GET TRACER
    // =========================================================

    private BulletTracer GetTracer(
        BulletTracer template,
        Pool pool)
    {
        while (
            pool.Available.Count > 0
        )
        {
            BulletTracer tracer =
                pool.Available.Dequeue();


            if (tracer != null)
            {
                return tracer;
            }
        }


        BulletTracer newTracer =
            Instantiate(
                template,
                transform
            );


        newTracer.gameObject.name =
            template.name +
            "_Pooled";


        newTracer.gameObject.SetActive(
            false
        );


        newTracer.Finished +=
            ReturnToPool;


        tracerPools.Add(
            newTracer,
            pool
        );


        return newTracer;
    }


    // =========================================================
    // RETURN
    // =========================================================

    private void ReturnToPool(
        BulletTracer tracer)
    {
        if (tracer == null)
            return;


        if (
            !tracerPools.TryGetValue(
                tracer,
                out Pool pool
            )
        )
        {
            Destroy(
                tracer.gameObject
            );

            return;
        }


        tracer.gameObject.SetActive(
            false
        );


        pool.Available.Enqueue(
            tracer
        );
    }
}