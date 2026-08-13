using UnityEngine;

public class MeleeAttackState : EnemyState
{
    private readonly EnemyMelee melee;

    private float attackTimer;

    public MeleeAttackState(Enemy enemy)
        : base(enemy)
    {
        melee = enemy.GetComponent<EnemyMelee>();
    }

    public override void Enter()
    {
        if (melee == null)
        {
            Debug.LogError(
                $"[{enemy.name}] MeleeAttackState: " +
                "EnemyMelee is missing."
            );

            return;
        }

        melee.StopMoving();

        attackTimer = 0f;

        Debug.Log(
            $"[{enemy.name}] State: Melee Attack"
        );
    }

    public override void Tick()
    {
        if (melee == null ||
            enemy.Vision == null)
        {
            return;
        }

        Transform player =
            enemy.Vision.Player;

        if (player == null)
            return;

        if (!enemy.Vision.CanSeePlayer())
        {
            enemy.StateMachine.ChangeState(
                new MeleeIdleState(enemy)
            );

            return;
        }

        float distance =
            Vector3.Distance(
                melee.transform.position,
                player.position
            );

        if (distance >
            melee.AttackDistance + 0.25f)
        {
            enemy.StateMachine.ChangeState(
                new MeleeChaseState(enemy)
            );

            return;
        }

        melee.StopMoving();
        melee.LookAtPlayer(player);

        attackTimer -= Time.deltaTime;

        if(attackTimer <= 0f)
{
            melee.StartAttackAnimation();

            attackTimer =
                melee.AttackCooldown;

            enemy.StateMachine.ChangeState(
                new MeleeBackstepState(enemy)
            );

            return;
        }
    }

    public override void Exit()
    {
        if (melee != null)
        {
            melee.StopMoving();
        }
    }
}