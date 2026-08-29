using UnityEngine;

public class MeleeDeadState : EnemyState
{
    private readonly EnemyMelee melee;

    public MeleeDeadState(Enemy enemy)
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

        melee.CancelAttack();
        melee.StopMoving();
    }

    public override void Tick()
    {
    }

    public override void Exit()
    {
    }
}