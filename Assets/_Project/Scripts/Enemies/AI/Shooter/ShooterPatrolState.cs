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
            return;
        }

        if (shooter.PatrolPoints == null ||
            shooter.PatrolPoints.Length == 0)
        {
            return;
        }

        shooter.SetPatrolIndex(0);
        shooter.MoveToCurrentPatrolPoint();

        waitTimer = 0f;
    }

    public override void Tick()
    {
        if (shooter == null)
            return;

        //Проверяет, увидел ли враг игрока
        if (enemy.Vision != null &&
            enemy.Vision.CanSeePlayer())
        {
            shooter.StopMoving();

            enemy.StateMachine.ChangeState(
                new ShooterAlertState(enemy)
            );

            return;
        }

        //Патруль
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