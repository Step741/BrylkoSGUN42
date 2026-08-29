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

    private void OnGetShell(
        ShellCasing shell)
    {
        //Ставит гильзу в новую позицию
        shell.transform.SetPositionAndRotation(
            spawnPosition,
            spawnRotation
        );

        shell.gameObject.SetActive(true);
    }

    private void OnReleaseShell(
        ShellCasing shell)
    {
        if (shell == null)
            return;

        shell.gameObject.SetActive(false);
    }

    private void OnDestroyShell(
        ShellCasing shell)
    {
        if (shell != null)
        {
            Destroy(shell.gameObject);
        }
    }

    public ShellCasing GetShell(
        Vector3 position,
        Quaternion rotation)
    {
        if (pool == null)
        {
            return null;
        }

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