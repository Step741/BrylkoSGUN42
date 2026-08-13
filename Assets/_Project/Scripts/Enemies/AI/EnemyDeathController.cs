using UnityEngine;

public class EnemyDeathController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;

    private Enemy enemy;
    private bool isDead;

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

        if (enemy == null)
            return;

        enemy.MarkAsDead();

        enemy.StateMachine.ChangeState(
            new ShooterDeadState(enemy)
        );
    }
}