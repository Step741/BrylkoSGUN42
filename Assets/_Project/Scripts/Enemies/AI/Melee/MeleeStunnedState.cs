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
            Debug.LogError(
                $"[{enemy.name}] MeleeStunnedState: " +
                "EnemyMelee is missing."
            );

            return;
        }

        // Полностью останавливаем движение
        // и отменяем текущую атаку.
        melee.CancelAttack();
        melee.StopMoving();

        stunTimer =
            melee.StunDuration;

        melee.PlayStunnedAnimation();

        Debug.Log(
            $"[{enemy.name}] State: Melee Stunned"
        );
    }

    public override void Tick()
    {
        if (melee == null)
            return;

        // Во время оглушения не двигаемся.
        melee.StopMoving();

        // Если враг уже умер,
        // существующая система Death сама разберётся.
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