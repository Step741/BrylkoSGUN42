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
            return;
        }

        coverPoint =
            shooter.FindBestCoverPoint();

        if (coverPoint == null)
        {
            enemy.StateMachine.ChangeState(
                new ShooterCombatState(enemy)
            );

            return;
        }

        reloadTimer = shooter.ReloadTime;

        shooter.MoveToCover(coverPoint);
    }

    public override void Tick()
    {
        if (shooter == null)
            return;

        if (shooter.Health == null)
            return;

        if (shooter.Health.IsDead)
            return;

        if (!shooter.HasReachedCover(coverPoint))
        {
            shooter.MoveToCover(coverPoint);
            return;
        }

        shooter.StopMoving();

        reloadTimer -= Time.deltaTime;

        if (reloadTimer <= 0f)
        {
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