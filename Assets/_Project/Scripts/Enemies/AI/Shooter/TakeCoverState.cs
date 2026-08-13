using UnityEngine;

public class ShooterTakeCoverState : EnemyState
{
    private readonly EnemyShooter shooter;

    private Transform coverPoint;
    private float reloadTimer;

    public ShooterTakeCoverState(Enemy enemy)
        : base(enemy)
    {
        shooter = enemy.GetComponent<EnemyShooter>();
    }

    public override void Enter()
    {
        if (shooter == null)
        {
            Debug.LogError(
                $"[{enemy.name}] ShooterTakeCoverState: " +
                "EnemyShooter is missing."
            );

            return;
        }

        coverPoint =
            shooter.FindBestCoverPoint();

        if (coverPoint == null)
        {
            Debug.LogWarning(
                $"[{enemy.name}] No valid cover point found."
            );

            enemy.StateMachine.ChangeState(
                new ShooterCombatState(enemy)
            );

            return;
        }

        reloadTimer = shooter.ReloadTime;

        shooter.MoveToCover(coverPoint);

        Debug.Log(
            $"[{enemy.name}] State: TakeCover -> " +
            $"moving to {coverPoint.name}"
        );
    }

    public override void Tick()
    {
        if (shooter == null)
            return;

        if (shooter.Health == null)
            return;

        // Смерть уже обрабатывается EnemyDeathController.
        if (shooter.Health.IsDead)
            return;

        // ==========================================
        // Двигаемся к укрытию.
        // ==========================================

        if (!shooter.HasReachedCover(coverPoint))
        {
            shooter.MoveToCover(coverPoint);
            return;
        }

        // ==========================================
        // Мы за укрытием.
        // Имитация перезарядки.
        // ==========================================

        shooter.StopMoving();

        reloadTimer -= Time.deltaTime;

        if (reloadTimer <= 0f)
        {
            Debug.Log(
                $"[{enemy.name}] Reload complete."
            );

            enemy.StateMachine.ChangeState(
                new ShooterCombatState(enemy)
            );
        }
    }

    public override void Exit()
    {
        if (shooter != null)
        {
            shooter.StopMoving();
        }
    }
}