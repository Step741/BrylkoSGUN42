using System.Collections.Generic;
using UnityEngine;

public class ImpactVfxPool : MonoBehaviour
{
    public static ImpactVfxPool Instance
    {
        get;
        private set;
    }

    private readonly Dictionary<
        ParticleSystem,
        Queue<ImpactVfx>
    > pools =
        new Dictionary<
            ParticleSystem,
            Queue<ImpactVfx>
        >();

    private readonly Dictionary<
        ImpactVfx,
        ParticleSystem
    > vfxPrefabs =
        new Dictionary<
            ImpactVfx,
            ParticleSystem
        >();

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

    public ImpactVfx Get(
        ParticleSystem prefab,
        Vector3 position,
        Quaternion rotation)
    {
        if (prefab == null)
            return null;

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

        vfx.Play(
            position,
            rotation
        );


        return vfx;
    }

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