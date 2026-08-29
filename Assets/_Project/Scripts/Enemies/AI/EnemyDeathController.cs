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

    [Inject]
    private void Construct(DropTable dropTable)
    {
        this.dropTable = dropTable;
    }

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

    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;

        SpawnDrop();

        if (enemy == null)
            return;

        enemy.MarkAsDead();

        enemy.StateMachine.ChangeState(
            new ShooterDeadState(enemy)
        );
    }

    private void SpawnDrop()
    {
        if (dropTable == null)
        {
            return;
        }

        if (dropConfig == null)
        {
            return;
        }

        dropTable.Roll(
            dropConfig,
            transform.position
        );
    }
}