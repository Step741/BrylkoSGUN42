using UnityEngine;
using Zenject;

public class EnemyDeathController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Health health;

    [Header("Drop")]
    [SerializeField]
    private DropConfig dropConfig;

    private Enemy enemy;

    private DropTable dropTable;

    private bool isDead;


    // =========================================================
    // ZENJECT
    // =========================================================

    [Inject]
    private void Construct(DropTable dropTable)
    {
        this.dropTable = dropTable;
    }


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        enemy = GetComponent<Enemy>();

        if (health == null)
        {
            health = GetComponent<Health>();
        }
    }


    private void OnEnable()
    {
        if (health != null)
        {
            health.Died += HandleDeath;
        }
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= HandleDeath;
        }
    }


    // =========================================================
    // DEATH
    // =========================================================

    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;


        // -----------------------------------------------------
        // DROP
        // -----------------------------------------------------

        SpawnDrop();


        // -----------------------------------------------------
        // EXISTING DEATH LOGIC
        // -----------------------------------------------------

        if (enemy == null)
            return;

        enemy.MarkAsDead();

        enemy.StateMachine.ChangeState(
            new ShooterDeadState(enemy)
        );
    }


    // =========================================================
    // DROP
    // =========================================================

    private void SpawnDrop()
    {
        if (dropTable == null)
        {
            Debug.LogWarning(
                $"[{name}] DropTable is not injected.",
                this
            );

            return;
        }

        if (dropConfig == null)
        {
            Debug.LogWarning(
                $"[{name}] DropConfig is not assigned.",
                this
            );

            return;
        }


        dropTable.Roll(
            dropConfig,
            transform.position
        );
    }
}