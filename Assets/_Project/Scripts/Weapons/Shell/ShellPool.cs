using UnityEngine;
using UnityEngine.Pool;

public class ShellPool : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField]
    private ShellCasing shellPrefab;

    [SerializeField]
    private int initialSize = 10;

    [SerializeField]
    private int maxSize = 100;


    private ObjectPool<ShellCasing> pool;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;


    private void Awake()
    {
        if (shellPrefab == null)
        {
            Debug.LogError(
                $"{name}: Shell Prefab is missing."
            );

            return;
        }


        pool = new ObjectPool<ShellCasing>(
            CreateShell,
            OnGetShell,
            OnReleaseShell,
            OnDestroyShell,
            true,
            initialSize,
            maxSize
        );
    }


    // =========================================================
    // CREATE
    // =========================================================

    private ShellCasing CreateShell()
    {
        ShellCasing shell =
            Instantiate(
                shellPrefab,
                transform
            );

        shell.SetPool(this);

        shell.gameObject.SetActive(false);

        return shell;
    }


    // =========================================================
    // GET
    // =========================================================

    private void OnGetShell(
        ShellCasing shell)
    {
        // Сначала ставим гильзу в новую позицию
        shell.transform.SetPositionAndRotation(
            spawnPosition,
            spawnRotation
        );

        // Только потом активируем
        shell.gameObject.SetActive(true);
    }


    // =========================================================
    // RELEASE
    // =========================================================

    private void OnReleaseShell(
        ShellCasing shell)
    {
        if (shell == null)
            return;

        shell.gameObject.SetActive(false);
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroyShell(
        ShellCasing shell)
    {
        if (shell != null)
        {
            Destroy(shell.gameObject);
        }
    }


    // =========================================================
    // PUBLIC API
    // =========================================================

    public ShellCasing GetShell(
        Vector3 position,
        Quaternion rotation)
    {
        if (pool == null)
        {
            Debug.LogError(
                $"{name}: Shell Pool is not initialized."
            );

            return null;
        }


        // Сохраняем новую позицию ДО pool.Get()
        spawnPosition = position;
        spawnRotation = rotation;


        ShellCasing shell =
            pool.Get();


        return shell;
    }


    public void Release(
        ShellCasing shell)
    {
        if (shell == null)
            return;

        if (pool == null)
            return;


        pool.Release(shell);
    }
}