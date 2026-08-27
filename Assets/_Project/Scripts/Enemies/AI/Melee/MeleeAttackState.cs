using UnityEngine;

public class MeleeAttackState : EnemyState
{
    private readonly EnemyMelee melee;

    private float attackTimer;


    public MeleeAttackState(Enemy enemy)
        : base(enemy)
    {
        melee =
            enemy.GetComponent<EnemyMelee>();
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


        attackTimer =
            0f;


        Debug.Log(
            $"[{enemy.name}] State: Melee Attack"
        );
    }


    public override void Tick()
    {
        if (
            melee == null ||
            enemy.Vision == null
        )
        {
            return;
        }


        Transform player =
            enemy.Vision.Player;


        if (player == null)
            return;


        // Если игрок потерян — прекращаем атаку.
        if (!enemy.Vision.CanSeePlayer())
        {
            melee.CancelAttack();


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


        // Если игрок успел уйти далеко,
        // прекращаем атаку и начинаем преследование.
        if (
            distance >
            melee.AttackDistance + 0.25f
        )
        {
            melee.CancelAttack();


            enemy.StateMachine.ChangeState(
                new MeleeChaseState(enemy)
            );

            return;
        }


        melee.StopMoving();


        melee.LookAtPlayer(
            player
        );


        // Пока идёт анимация атаки —
        // ждём Animation Event.
        if (
            melee.IsAttackAnimationPlaying()
        )
        {
            return;
        }


        attackTimer -=
            Time.deltaTime;


        // Запускаем следующую атаку.
        if (attackTimer <= 0f)
        {
            melee.StartAttackAnimation();


            attackTimer =
                melee.AttackCooldown;
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