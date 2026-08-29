using System.Collections;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnMode
    {
        Timer,
        Trigger
    }

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

    [Header("Projectile Pool")]

    [SerializeField]
    private SpitProjectilePool spitProjectilePool;

    [Header("Wave")]

    [SerializeField]
    private int enemiesPerWave = 3;

    [SerializeField]
    private float waveInterval = 10f;

    [SerializeField]
    [Min(0f)]
    private float spawnInterval = 1f;

    [SerializeField]
    private bool spawnFirstWaveImmediately =
        true;

    private IEnemyFactory enemyFactory;

    private float timer;

    private bool waveActive;

    private Coroutine waveCoroutine;

    [Inject]
    private void Construct(
        IEnemyFactory enemyFactory)
    {
        this.enemyFactory =
            enemyFactory;
    }

    private void Start()
    {
        if (!ValidateSetup())
            return;

        if (
            spawnMode ==
            SpawnMode.Timer
        )
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
        if (
            spawnMode !=
            SpawnMode.Timer
        )
        {
            return;
        }

        //Пока текущая волна создаётся, не запускат следующую
        if (waveActive)
            return;

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
        if (
            spawnMode !=
            SpawnMode.Trigger
        )
        {
            return;
        }

        if (waveActive)
            return;

        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        SpawnWave();
    }


    private void OnDisable()
    {
        CancelWave();
    }

    public void SpawnWave()
    {
        if (waveActive)
            return;

        if (!ValidateSetup())
            return;

        waveCoroutine =
            StartCoroutine(
                SpawnWaveRoutine()
            );
    }


    private IEnumerator SpawnWaveRoutine()
    {
        waveActive = true;

        for (
            int i = 0;
            i < enemiesPerWave;
            i++
        )
        {
            SpawnEnemy(i);

            if (
                i <
                enemiesPerWave - 1
            )
            {
                if (spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(
                        spawnInterval
                    );
                }
                else
                {
                    yield return null;
                }
            }
        }

        waveActive = false;

        waveCoroutine = null;
    }

    private void CancelWave()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(
                waveCoroutine
            );

            waveCoroutine = null;
        }

        waveActive = false;
    }

    private void SpawnEnemy(
        int index)
    {
        Transform spawnPoint =
            GetSpawnPoint(index);

        if (spawnPoint == null)
            return;

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

        int pointIndex =
            index % spawnPoints.Length;

        return spawnPoints[
            pointIndex
        ];
    }
    private bool ValidateSetup()
    {
        if (enemyFactory == null)
        {
            return false;
        }


        if (enemyPrefab == null)
        {
            return false;
        }


        if (
            spawnPoints == null ||
            spawnPoints.Length < 2
        )
        {
            return false;
        }


        if (enemiesPerWave <= 0)
        {
            return false;
        }

        if (
            enemyType ==
            EnemySpawnType.Shooter &&
            spitProjectilePool == null
        )
        {
        }

        return true;
    }

    // GIZMOS
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null)
            return;

        for (
            int i = 0;
            i < spawnPoints.Length;
            i++
        )
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