using UnityEngine;

public class ShooterSearchState : EnemyState
{
    private readonly EnemyShooter shooter;

    private const float SearchTime = 3f;

    private float searchTimer;
    private bool reachedLastKnownPosition;

    public ShooterSearchState(Enemy enemy)
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

        searchTimer = 0f;
        reachedLastKnownPosition = false;

        if (shooter.Agent != null &&
            shooter.Agent.isOnNavMesh)
        {
            shooter.Agent.isStopped = false;

            shooter.Agent.SetDestination(
                shooter.LastKnownPlayerPosition
            );
        }
    }

    public override void Tick()
    {
        if (shooter == null ||
            enemy.Vision == null)
        {
            return;
        }

        // Игрок снова найден.
        if (enemy.Vision.CanSeePlayer())
        {
            enemy.StateMachine.ChangeState(
                new ShooterCombatState(enemy)
            );

            return;
        }

        //Идёт к последней известной позиции
        if (!reachedLastKnownPosition)
        {
            if (shooter.HasReachedPosition(
                    shooter.LastKnownPlayerPosition))
            {
                shooter.StopMoving();
                reachedLastKnownPosition = true;
            }

            return;
        }

        //Осматривается
        searchTimer += Time.deltaTime;

        if (searchTimer >= SearchTime)
        {
            enemy.StateMachine.ChangeState(
                new ShooterPatrolState(enemy)
            );
        }
    }

    public override void Exit()
    {
        if (shooter != null)
            shooter.StopMoving();
    }
}