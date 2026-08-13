using UnityEngine;

public class ShooterPatrolState : EnemyState
{
    private readonly EnemyShooter shooter;

    private float waitTimer;

    public ShooterPatrolState(Enemy enemy)
        : base(enemy)
    {
        shooter = enemy.GetComponent<EnemyShooter>();
    }

    public override void Enter()
    {
        if (shooter == null)
        {
            Debug.LogError(
                $"[{enemy.name}] ShooterPatrolState: EnemyShooter is missing."
            );

            return;
        }

        if (shooter.PatrolPoints == null ||
            shooter.PatrolPoints.Length == 0)
        {
            Debug.LogWarning(
                $"[{enemy.name}] No patrol points assigned."
            );

            return;
        }

        shooter.SetPatrolIndex(0);
        shooter.MoveToCurrentPatrolPoint();

        waitTimer = 0f;

        Debug.Log(
            $"[{enemy.name}] State: Shooter Patrol"
        );
    }

    public override void Tick()
    {
        if (shooter == null)
            return;

        // ==========================================
        // 1. Проверяем, увидел ли враг игрока.
        // ==========================================

        if (enemy.Vision != null &&
            enemy.Vision.CanSeePlayer())
        {
            shooter.StopMoving();

            enemy.StateMachine.ChangeState(
                new ShooterAlertState(enemy)
            );

            return;
        }

        // ==========================================
        // 2. Обычный патруль.
        // ==========================================

        if (shooter.PatrolPoints == null ||
            shooter.PatrolPoints.Length == 0)
        {
            return;
        }

        if (!shooter.HasReachedCurrentPatrolPoint())
            return;

        shooter.StopMoving();

        waitTimer += Time.deltaTime;

        if (waitTimer < shooter.WaitAtPoint)
            return;

        waitTimer = 0f;

        shooter.MoveToNextPatrolPoint();
    }

    public override void Exit()
    {
        if (shooter != null)
            shooter.StopMoving();
    }
}