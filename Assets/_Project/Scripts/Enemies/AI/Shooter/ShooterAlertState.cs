using UnityEngine;

public class ShooterAlertState : EnemyState
{
    private readonly EnemyShooter shooter;

    private readonly EnemySoundController
        enemySoundController;


    public ShooterAlertState(Enemy enemy)
        : base(enemy)
    {
        shooter =
            enemy.GetComponent<EnemyShooter>();

        enemySoundController =
            enemy.GetComponent<EnemySoundController>();
    }


    public override void Enter()
    {
        if (shooter == null)
        {
            return;
        }


        if (enemy.Vision == null)
        {
            return;
        }


        if (enemySoundController != null)
        {
            enemySoundController.PlayAlert();
        }
    }


    public override void Tick()
    {
        if (shooter == null)
            return;

        if (enemy.Vision == null)
            return;


        Transform player =
            enemy.Vision.Player;


        if (player == null)
        {
            shooter.StopMoving();

            return;
        }


        float distance =
            Vector3.Distance(
                shooter.transform.position,
                player.position
            );


        if (distance > shooter.AttackDistance)
        {
            if (
                shooter.Agent != null &&
                shooter.Agent.isOnNavMesh
            )
            {
                shooter.Agent.isStopped =
                    false;

                shooter.Agent.SetDestination(
                    player.position
                );
            }

            return;
        }


        shooter.StopMoving();


        enemy.StateMachine.ChangeState(
            new ShooterCombatState(enemy)
        );
    }


    public override void Exit()
    {
        if (shooter != null)
        {
            shooter.StopMoving();
        }
    }
}