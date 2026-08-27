using System.Collections.Generic;
using UnityEngine;

public class ImpactVfxPool : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static ImpactVfxPool Instance
    {
        get;
        private set;
    }


    // =========================================================
    // POOLS
    // =========================================================

    private readonly Dictionary<
        ParticleSystem,
        Queue<ImpactVfx>
    > pools =
        new Dictionary<
            ParticleSystem,
            Queue<ImpactVfx>
        >();


    // =========================================================
    // VFX OWNERS
    // =========================================================

    private readonly Dictionary<
        ImpactVfx,
        ParticleSystem
    > vfxPrefabs =
        new Dictionary<
            ImpactVfx,
            ParticleSystem
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

    public ImpactVfx Get(
        ParticleSystem prefab,
        Vector3 position,
        Quaternion rotation)
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
                new Queue<ImpactVfx>()
            );
        }


        Queue<ImpactVfx> pool =
            pools[prefab];


        ImpactVfx vfx =
            null;


        // =====================================================
        // FIND AVAILABLE VFX
        // =====================================================

        while (
            pool.Count > 0 &&
            vfx == null
        )
        {
            ImpactVfx candidate =
                pool.Dequeue();


            if (candidate != null)
            {
                vfx =
                    candidate;
            }
        }


        // =====================================================
        // CREATE NEW
        // =====================================================

        if (vfx == null)
        {
            ParticleSystem vfxObject =
                Instantiate(
                    prefab,
                    transform
                );


            vfx =
                vfxObject.GetComponent<
                    ImpactVfx
                >();


            if (vfx == null)
            {
                vfx =
                    vfxObject.gameObject.AddComponent<
                        ImpactVfx
                    >();
            }


            vfxPrefabs[
                vfx
            ] =
                prefab;
        }


        // =====================================================
        // ACTIVATE
        // =====================================================

        vfx.Play(
            position,
            rotation
        );


        return vfx;
    }


    // =========================================================
    // RETURN
    // =========================================================

    public void Return(
        ImpactVfx vfx)
    {
        if (vfx == null)
            return;


        if (
            !vfxPrefabs.TryGetValue(
                vfx,
                out ParticleSystem prefab
            )
        )
        {
            vfx.ReturnToPool();

            return;
        }


        if (
            !pools.TryGetValue(
                prefab,
                out Queue<ImpactVfx> pool
            )
        )
        {
            pool =
                new Queue<ImpactVfx>();


            pools.Add(
                prefab,
                pool
            );
        }


        vfx.ReturnToPool();

        pool.Enqueue(
            vfx
        );
    }
}