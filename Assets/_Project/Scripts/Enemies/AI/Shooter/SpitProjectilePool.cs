using UnityEngine;
using UnityEngine.Pool;


public class SpitProjectilePool : MonoBehaviour
{
    // ==========================================
    // POOL
    // ==========================================

    [Header("Pool")]

    [SerializeField]
    private SpitProjectile projectilePrefab;

    [SerializeField]
    private int initialSize = 10;

    [SerializeField]
    private int maxSize = 50;


    // ==========================================
    // POOL INSTANCE
    // ==========================================

    private ObjectPool<SpitProjectile> pool;


    // ==========================================
    // UNITY
    // ==========================================

    private void Awake()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError(
                $"[{name}] Spit Projectile Prefab is missing."
            );

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


    // ==========================================
    // CREATE
    // ==========================================

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


    // ==========================================
    // ON GET
    // ==========================================

    private void OnGetProjectile(
        SpitProjectile projectile
    )
    {
        // Важно:
        // Здесь специально ничего не активируем.
        // Сначала объект будет перемещён
        // в SpitOrigin, затем включён.
    }


    // ==========================================
    // ON RELEASE
    // ==========================================

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


    // ==========================================
    // ON DESTROY
    // ==========================================

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


    // ==========================================
    // GET PROJECTILE
    // ==========================================

    public SpitProjectile GetProjectile(
        Vector3 position,
        Quaternion rotation
    )
    {
        if (pool == null)
        {
            Debug.LogError(
                $"[{name}] Spit Projectile Pool is not initialized."
            );

            return null;
        }


        SpitProjectile projectile =
            pool.Get();


        if (projectile == null)
            return null;


        // ==========================================
        // СНАЧАЛА СТАВИМ В НУЖНУЮ ПОЗИЦИЮ
        // ==========================================

        projectile.transform.SetPositionAndRotation(
            position,
            rotation
        );


        // ==========================================
        // И ТОЛЬКО ПОТОМ АКТИВИРУЕМ
        // ==========================================

        projectile.gameObject.SetActive(
            true
        );


        return projectile;
    }


    // ==========================================
    // RELEASE
    // ==========================================

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