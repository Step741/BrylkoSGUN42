using UnityEngine;

public class MeleeBackstepState : EnemyState
{
    private readonly EnemyMelee melee;

    private Transform player;

    private Vector3 targetPosition;

    private float timer;

    private const float BackstepDistance = 2f;
    private const float BackstepDuration = 0.6f;

    public MeleeBackstepState(Enemy enemy)
        : base(enemy)
    {
        melee = enemy.GetComponent<EnemyMelee>();
    }

    public override void Enter()
    {
        if (melee == null)
        {
            return;
        }

        player =
            enemy.Vision != null
                ? enemy.Vision.Player
                : null;

        timer = BackstepDuration;

        if (player == null)
        {
            enemy.StateMachine.ChangeState(
                new MeleeIdleState(enemy)
            );

            return;
        }

        Vector3 direction =
            melee.transform.position -
            player.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction =
                -melee.transform.forward;

            direction.y = 0f;
        }

        direction.Normalize();

        targetPosition =
            melee.transform.position +
            direction * BackstepDistance;

        if (melee.Agent != null &&
            melee.Agent.isOnNavMesh)
        {
            melee.Agent.isStopped = false;

            melee.Agent.SetDestination(
                targetPosition
            );
        }
    }

    public override void Tick()
    {
        if (melee == null)
            return;

        timer -= Time.deltaTime;

        if (player != null)
        {
            melee.LookAtPlayer(player);
        }

        if (timer <= 0f)
        {
            ReturnToChase();
            return;
        }

        if (melee.Agent == null ||
            !melee.Agent.isOnNavMesh)
        {
            ReturnToChase();
            return;
        }

        if (!melee.Agent.pathPending &&
            melee.Agent.remainingDistance <= 0.2f)
        {
            ReturnToChase();
        }
    }

    private void ReturnToChase()
    {
        enemy.StateMachine.ChangeState(
            new MeleeChaseState(enemy)
        );
    }

    public override void Exit()
    {
        if (melee != null)
        {
            melee.StopMoving();
        }
    }
}