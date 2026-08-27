using System.Collections.Generic;
using UnityEngine;

public class DecalPool : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static DecalPool Instance
    {
        get;
        private set;
    }


    // =========================================================
    // POOLS
    // =========================================================

    private readonly Dictionary<
        GameObject,
        Queue<ImpactDecal>
    > pools =
        new Dictionary<
            GameObject,
            Queue<ImpactDecal>
        >();


    // =========================================================
    // DECAL OWNERS
    // =========================================================

    private readonly Dictionary<
        ImpactDecal,
        GameObject
    > decalPrefabs =
        new Dictionary<
            ImpactDecal,
            GameObject
        >();


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


    // =========================================================
    // GET
    // =========================================================

    public ImpactDecal Get(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        float lifetime,
        Transform followTarget)
    {
        if (prefab == null)
            return null;


        // =====================================================
        // CREATE POOL
        // =====================================================

        if (
            !pools.ContainsKey(
                prefab
            )
        )
        {
            pools.Add(
                prefab,
                new Queue<ImpactDecal>()
            );
        }


        Queue<ImpactDecal> pool =
            pools[prefab];


        ImpactDecal decal =
            null;


        // =====================================================
        // FIND AVAILABLE DECAL
        // =====================================================

        while (
            pool.Count > 0 &&
            decal == null
        )
        {
            ImpactDecal candidate =
                pool.Dequeue();


            if (candidate != null)
            {
                decal =
                    candidate;
            }
        }


        // =====================================================
        // CREATE NEW
        // =====================================================

        if (decal == null)
        {
            GameObject decalObject =
                Instantiate(
                    prefab,
                    transform
                );


            decal =
                decalObject.GetComponent<
                    ImpactDecal
                >();


            if (decal == null)
            {
                decal =
                    decalObject.AddComponent<
                        ImpactDecal
                    >();
            }


            decalPrefabs[
                decal
            ] =
                prefab;
        }


        // =====================================================
        // ACTIVATE
        // =====================================================

        decal.Activate(
            position,
            rotation,
            lifetime,
            followTarget
        );


        return decal;
    }


    // =========================================================
    // RETURN
    // =========================================================

    public void Return(
        ImpactDecal decal)
    {
        if (decal == null)
            return;


        if (
            !decalPrefabs.TryGetValue(
                decal,
                out GameObject prefab
            )
        )
        {
            decal.ReturnToPool();

            return;
        }


        if (
            !pools.TryGetValue(
                prefab,
                out Queue<ImpactDecal> pool
            )
        )
        {
            pool =
                new Queue<ImpactDecal>();


            pools.Add(
                prefab,
                pool
            );
        }


        decal.ReturnToPool();

        pool.Enqueue(
            decal
        );
    }
}