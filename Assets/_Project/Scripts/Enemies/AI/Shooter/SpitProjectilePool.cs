using UnityEngine;
using UnityEngine.Pool;


public class SpitProjectilePool : MonoBehaviour
{
    [Header("Pool")]

    [SerializeField]
    private SpitProjectile projectilePrefab;

    [SerializeField]
    private int initialSize = 10;

    [SerializeField]
    private int maxSize = 50;

    private ObjectPool<SpitProjectile> pool;

    private void Awake()
    {
        if (projectilePrefab == null)
        {
            return;
        }


        pool =
            new ObjectPool<SpitProjectile>(
                CreateProjectile,
                OnGetProjectile,
                OnReleaseProjectile,
                OnDestroyProjectile,
                collectionCheck: true,
                defaultCapacity: initialSize,
                maxSize: maxSize
            );
    }

    private SpitProjectile CreateProjectile()
    {
        SpitProjectile projectile =
            Instantiate(
                projectilePrefab,
                transform
            );


        projectile.SetPool(
            this
        );


        projectile.gameObject.SetActive(
            false
        );


        return projectile;
    }

    private void OnGetProjectile(
        SpitProjectile projectile
    )
    {
    }

    private void OnReleaseProjectile(
        SpitProjectile projectile
    )
    {
        if (projectile == null)
            return;


        projectile.gameObject.SetActive(
            false
        );
    }

    private void OnDestroyProjectile(
        SpitProjectile projectile
    )
    {
        if (projectile != null)
        {
            Destroy(
                projectile.gameObject
            );
        }
    }

    public SpitProjectile GetProjectile(
        Vector3 position,
        Quaternion rotation
    )
    {
        if (pool == null)
        {
            return null;
        }


        SpitProjectile projectile =
            pool.Get();


        if (projectile == null)
            return null;

        projectile.transform.SetPositionAndRotation(
            position,
            rotation
        );

        projectile.gameObject.SetActive(
            true
        );


        return projectile;
    }

    public void Release(
        SpitProjectile projectile
    )
    {
        if (projectile == null)
            return;

        if (pool == null)
            return;


        pool.Release(
            projectile
        );
    }
}