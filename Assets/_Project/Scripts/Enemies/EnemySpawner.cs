using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnMode
    {
        Timer,
        Trigger
    }


    // ==========================================
    // SPAWN
    // ==========================================

    [Header("Spawn")]

    [SerializeField]
    private SpawnMode spawnMode =
        SpawnMode.Timer;

    [SerializeField]
    private EnemySpawnType enemyType =
        EnemySpawnType.Shooter;

    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField]
    private Transform[] spawnPoints;


    // ==========================================
    // PROJECTILE POOL
    // ==========================================

    [Header("Projectile Pool")]

    [SerializeField]
    private SpitProjectilePool spitProjectilePool;


    // ==========================================
    // WAVE
    // ==========================================

    [Header("Wave")]

    [SerializeField]
    private int enemiesPerWave = 3;

    [SerializeField]
    private float waveInterval = 10f;

    [SerializeField]
    private bool spawnFirstWaveImmediately =
        true;


    // ==========================================
    // DEPENDENCIES
    // ==========================================

    private IEnemyFactory enemyFactory;


    // ==========================================
    // STATE
    // ==========================================

    private float timer;

    private bool waveActive;


    // ==========================================
    // INJECTION
    // ==========================================

    [Inject]
    private void Construct(
        IEnemyFactory enemyFactory)
    {
        this.enemyFactory =
            enemyFactory;
    }


    // ==========================================
    // UNITY
    // ==========================================

    private void Start()
    {
        if (!ValidateSetup())
            return;

        if (spawnMode ==
            SpawnMode.Timer)
        {
            if (spawnFirstWaveImmediately)
            {
                SpawnWave();
            }

            timer = waveInterval;
        }
    }


    private void Update()
    {
        if (spawnMode !=
            SpawnMode.Timer)
        {
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnWave();

            timer = waveInterval;
        }
    }


    private void OnTriggerEnter(
        Collider other)
    {
        if (spawnMode !=
            SpawnMode.Trigger)
        {
            return;
        }

        if (waveActive)
            return;

        if (!other.CompareTag("Player"))
            return;

        SpawnWave();
    }


    // ==========================================
    // SPAWN WAVE
    // ==========================================

    public void SpawnWave()
    {
        if (waveActive)
            return;

        if (!ValidateSetup())
            return;

        waveActive = true;

        for (
            int i = 0;
            i < enemiesPerWave;
            i++)
        {
            SpawnEnemy(i);
        }

        waveActive = false;

        Debug.Log(
            $"[{name}] Wave spawned: " +
            $"{enemiesPerWave} enemies."
        );
    }


    // ==========================================
    // SPAWN ENEMY
    // ==========================================

    private void SpawnEnemy(
        int index)
    {
        Transform spawnPoint =
            GetSpawnPoint(index);

        if (spawnPoint == null)
            return;


        // Создаём врага через текущую фабрику.
        Enemy enemy =
            enemyFactory.Create(
                enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        if (enemy == null)
            return;


        enemy.name =
            $"{enemyType}_Enemy_{index + 1}";


        // ==========================================
        // SHOOTER PROJECTILE POOL
        // ==========================================

        // Если заспавненный враг является стрелком,
        // передаём ему общий пул снарядов со сцены.
        if (spitProjectilePool != null)
        {
            EnemyShooter shooter =
                enemy.GetComponentInChildren<
                    EnemyShooter
                >();

            if (shooter != null)
            {
                shooter.SetProjectilePool(
                    spitProjectilePool
                );
            }
        }
    }


    // ==========================================
    // SPAWN POINT
    // ==========================================

    private Transform GetSpawnPoint(
        int index)
    {
        if (
            spawnPoints == null ||
            spawnPoints.Length == 0
        )
        {
            return null;
        }


        // Распределяем врагов по точкам
        // по кругу:
        //
        // 0 → Point 0
        // 1 → Point 1
        // 2 → Point 0
        // 3 → Point 1
        //

        int pointIndex =
            index % spawnPoints.Length;

        return spawnPoints[
            pointIndex
        ];
    }


    // ==========================================
    // VALIDATION
    // ==========================================

    private bool ValidateSetup()
    {
        if (enemyFactory == null)
        {
            Debug.LogError(
                $"[{name}] IEnemyFactory is not injected."
            );

            return false;
        }


        if (enemyPrefab == null)
        {
            Debug.LogError(
                $"[{name}] Enemy Prefab is missing."
            );

            return false;
        }


        if (
            spawnPoints == null ||
            spawnPoints.Length < 2
        )
        {
            Debug.LogError(
                $"[{name}] EnemySpawner requires " +
                "at least 2 spawn points."
            );

            return false;
        }


        if (enemiesPerWave <= 0)
        {
            Debug.LogError(
                $"[{name}] Enemies Per Wave must be > 0."
            );

            return false;
        }


        // Пул нужен только для врага-стрелка.
        if (
            enemyType ==
            EnemySpawnType.Shooter &&
            spitProjectilePool == null
        )
        {
            Debug.LogWarning(
                $"[{name}] SpitProjectilePool is not assigned. " +
                "Spawned EnemyShooter will not be able to fire."
            );
        }

        return true;
    }


    // ==========================================
    // GIZMOS
    // ==========================================

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null)
            return;

        for (
            int i = 0;
            i < spawnPoints.Length;
            i++)
        {
            Transform point =
                spawnPoints[i];

            if (point == null)
                continue;

            Gizmos.DrawWireSphere(
                point.position,
                0.35f
            );

            Gizmos.DrawLine(
                point.position,
                point.position +
                point.forward
            );
        }
    }
}