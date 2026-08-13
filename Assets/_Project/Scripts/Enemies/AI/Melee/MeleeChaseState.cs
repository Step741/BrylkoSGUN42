using UnityEngine;

public class MeleeChaseState : EnemyState
{
    private readonly EnemyMelee melee;

    public MeleeChaseState(Enemy enemy)
        : base(enemy)
    {
        melee = enemy.GetComponent<EnemyMelee>();
    }

    public override void Enter()
    {
        if (melee == null)
        {
            Debug.LogError(
                $"[{enemy.name}] MeleeChaseState: EnemyMelee is missing."
            );

            return;
        }

        Debug.Log(
            $"[{enemy.name}] State: Melee Chase"
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

        // Игрок потерян.
        if (!enemy.Vision.CanSeePlayer())
        {
            melee.StopMoving();

            enemy.StateMachine.ChangeState(
                new MeleeIdleState(enemy)
            );

            return;
        }

        // Игрок в радиусе атаки.
        if (melee.IsPlayerInAttackRange(player))
        {
            melee.StopMoving();

            enemy.StateMachine.ChangeState(
                new MeleeAttackState(enemy)
            );

            return;
        }

        // Продолжаем преследование.
        melee.MoveToPlayer(player);
    }

    public override void Exit()
    {
        if (melee != null)
            melee.StopMoving();
    }
}