using UnityEngine;

public class ShooterBackstepState : EnemyState
{
    private readonly EnemyShooter shooter;

    private Transform player;

    private Vector3 targetPosition;

    private float timer;

    private const float BackstepDistance = 3f;
    private const float BackstepDuration = 0.8f;

    public ShooterBackstepState(Enemy enemy)
        : base(enemy)
    {
        shooter = enemy.GetComponent<EnemyShooter>();
    }

    public override void Enter()
    {
        if (shooter == null)
        {
            Debug.LogError(
                $"[{enemy.name}] ShooterBackstepState: EnemyShooter is missing."
            );

            return;
        }

        if (enemy.Vision == null)
        {
            Debug.LogError(
                $"[{enemy.name}] ShooterBackstepState: EnemyVision is missing."
            );

            return;
        }

        player = enemy.Vision.Player;

        timer = BackstepDuration;

        if (player == null)
        {
            enemy.StateMachine.ChangeState(
                new ShooterPatrolState(enemy)
            );

            return;
        }

        Vector3 direction =
            shooter.transform.position -
            player.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = -shooter.transform.forward;
            direction.y = 0f;
        }

        direction.Normalize();

        targetPosition =
            shooter.transform.position +
            direction * BackstepDistance;

        if (shooter.Agent != null &&
            shooter.Agent.isOnNavMesh)
        {
            shooter.Agent.isStopped = false;

            shooter.Agent.SetDestination(
                targetPosition
            );
        }

        Debug.Log(
            $"[{enemy.name}] State: Backstep"
        );
    }

    public override void Tick()
    {
        if (shooter == null)
            return;

        timer -= Time.deltaTime;

        if (player != null)
        {
            LookAtPlayer(player);
        }

        if (timer <= 0f)
        {
            ReturnToCombat();
            return;
        }

        if (shooter.Agent == null ||
            !shooter.Agent.isOnNavMesh)
        {
            ReturnToCombat();
            return;
        }

        if (!shooter.Agent.pathPending &&
            shooter.Agent.remainingDistance <= 0.2f)
        {
            ReturnToCombat();
        }
    }

    private void ReturnToCombat()
    {
        enemy.StateMachine.ChangeState(
            new ShooterCombatState(enemy)
        );
    }

    private void LookAtPlayer(Transform target)
    {
        Vector3 direction =
            target.position -
            shooter.transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        shooter.transform.rotation =
            Quaternion.Slerp(
                shooter.transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
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