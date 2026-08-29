using System.Collections.Generic;
using UnityEngine;

public class DecalPool : MonoBehaviour
{
    public static DecalPool Instance
    {
        get;
        private set;
    }

    private readonly Dictionary<
        GameObject,
        Queue<ImpactDecal>
    > pools =
        new Dictionary<
            GameObject,
            Queue<ImpactDecal>
        >();

    private readonly Dictionary<
        ImpactDecal,
        GameObject
    > decalPrefabs =
        new Dictionary<
            ImpactDecal,
            GameObject
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

    public ImpactDecal Get(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        float lifetime,
        Transform followTarget)
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
                new Queue<ImpactDecal>()
            );
        }


        Queue<ImpactDecal> pool =
            pools[prefab];


        ImpactDecal decal =
            null;

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

        decal.Activate(
            position,
            rotation,
            lifetime,
            followTarget
        );


        return decal;
    }

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