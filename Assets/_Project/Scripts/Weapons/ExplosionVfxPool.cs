using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ExplosionVfxPool : MonoBehaviour
{
    private static ExplosionVfxPool instance;

    private readonly Dictionary<
        GameObject,
        ObjectPool<GameObject>
    > pools = new();

    private void Awake()
    {
        if (instance != null &&
            instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public static void Play(
        GameObject prefab,
        Vector3 position,
        float lifetime)
    {
        if (instance == null)
        {
            Debug.LogError(
                "ExplosionVfxPool is missing in scene."
            );

            return;
        }

        if (prefab == null)
            return;

        if (!instance.pools.TryGetValue(
                prefab,
                out ObjectPool<GameObject> pool))
        {
            pool =
                instance.CreatePool(prefab);

            instance.pools.Add(
                prefab,
                pool
            );
        }

        GameObject effect =
            pool.Get();

        effect.transform.SetPositionAndRotation(
            position,
            Quaternion.identity
        );

        ExplosionVfx vfx =
            effect.GetComponent<ExplosionVfx>();

        if (vfx == null)
        {
            Debug.LogError(
                $"{prefab.name}: " +
                "ExplosionVfx component is missing."
            );

            pool.Release(effect);
            return;
        }

        vfx.Play(lifetime);

        instance.StartCoroutine(
            instance.ReleaseAfterTime(
                pool,
                effect,
                lifetime
            )
        );
    }

    private ObjectPool<GameObject> CreatePool(
        GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            () =>
            {
                GameObject obj =
                    Instantiate(prefab);

                obj.SetActive(false);

                return obj;
            },

            obj =>
            {
                obj.SetActive(true);
            },

            obj =>
            {
                obj.SetActive(false);
            },

            obj =>
            {
                Destroy(obj);
            },

            false,
            4,
            32
        );
    }

    private System.Collections.IEnumerator ReleaseAfterTime(
        ObjectPool<GameObject> pool,
        GameObject effect,
        float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        if (effect != null)
        {
            pool.Release(effect);
        }
    }
}