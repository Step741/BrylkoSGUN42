using UnityEngine;

public class MeleeStunnedState : EnemyState
{
    private readonly EnemyMelee melee;

    private float stunTimer;

    public MeleeStunnedState(Enemy enemy)
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

        //Полностью останавливает движение и отменяет текущую атаку
        melee.CancelAttack();
        melee.StopMoving();

        stunTimer =
            melee.StunDuration;

        melee.PlayStunnedAnimation();
    }

    public override void Tick()
    {
        if (melee == null)
            return;

        melee.StopMoving();

        if (melee.Health != null &&
            melee.Health.IsDead)
        {
            return;
        }

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            enemy.StateMachine.ChangeState(
                new MeleeChaseState(enemy)
            );
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