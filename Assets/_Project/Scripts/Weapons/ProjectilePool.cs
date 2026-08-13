using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField]
    private GrenadeProjectile projectilePrefab;

    [SerializeField]
    private int initialSize = 4;

    [SerializeField]
    private int maxSize = 16;

    private ObjectPool<GrenadeProjectile> pool;


    private void Awake()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError(
                $"{name}: Projectile Prefab is missing."
            );

            return;
        }

        pool = new ObjectPool<GrenadeProjectile>(
            CreateProjectile,
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile,
            true,
            initialSize,
            maxSize
        );
    }


    private GrenadeProjectile CreateProjectile()
    {
        GrenadeProjectile projectile =
            Instantiate(
                projectilePrefab,
                transform
            );

        projectile.SetPool(this);

        projectile.gameObject.SetActive(false);

        return projectile;
    }


    private void OnGetProjectile(
        GrenadeProjectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }


    private void OnReleaseProjectile(
        GrenadeProjectile projectile)
    {
        if (projectile == null)
            return;

        projectile.gameObject.SetActive(false);
    }


    private void OnDestroyProjectile(
        GrenadeProjectile projectile)
    {
        if (projectile != null)
        {
            Destroy(projectile.gameObject);
        }
    }


    public GrenadeProjectile GetProjectile(
        Vector3 position,
        Quaternion rotation)
    {
        if (pool == null)
        {
            Debug.LogError(
                $"{name}: Projectile Pool is not initialized."
            );

            return null;
        }

        GrenadeProjectile projectile =
            pool.Get();

        projectile.transform.SetPositionAndRotation(
            position,
            rotation
        );

        return projectile;
    }


    public void Release(
        GrenadeProjectile projectile)
    {
        if (projectile == null)
            return;

        if (pool == null)
            return;

        pool.Release(projectile);
    }
}