using UnityEngine;

public class MeleeIdleState : EnemyState
{
    private readonly EnemyMelee melee;

    public MeleeIdleState(Enemy enemy)
        : base(enemy)
    {
        melee = enemy.GetComponent<EnemyMelee>();
    }

    public override void Enter()
    {
        if (melee == null)
        {
            Debug.LogError(
                $"[{enemy.name}] MeleeIdleState: EnemyMelee is missing."
            );

            return;
        }

        melee.StopMoving();

        Debug.Log(
            $"[{enemy.name}] State: Melee Idle"
        );
    }

    public override void Tick()
    {
        if (melee == null)
            return;

        if (enemy.Vision == null)
            return;

        if (!enemy.Vision.CanSeePlayer())
            return;

        enemy.StateMachine.ChangeState(
            new MeleeChaseState(enemy)
        );
    }

    public override void Exit()
    {
        if (melee != null)
            melee.StopMoving();
    }
}