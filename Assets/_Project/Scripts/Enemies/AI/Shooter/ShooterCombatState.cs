using UnityEngine;


public class ShooterCombatState : EnemyState
{
    private readonly EnemyShooter shooter;

    private readonly EnemySoundController
        enemySoundController;


    private float attackTimer;

    private int attackCount;

    private const float MinAttackDistance =
        10f;

    private const int AttacksBeforeBackstep =
        3;


    public ShooterCombatState(
        Enemy enemy
    )
        : base(enemy)
    {
        shooter =
            enemy.GetComponent<EnemyShooter>();

        enemySoundController =
            enemy.GetComponent<
                EnemySoundController
            >();
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


        shooter.StopMoving();


        attackTimer =
            0f;

        attackCount =
            0;
    }

    public override void Tick()
    {
        if (
            shooter == null ||
            enemy.Vision == null
        )
        {
            return;
        }


        Transform player =
            enemy.Vision.Player;


        if (player == null)
            return;

        if (shooter.NeedsToTakeCover())
        {
            enemy.StateMachine.ChangeState(
                new ShooterTakeCoverState(
                    enemy
                )
            );

            return;
        }

        if (enemy.Vision.CanSeePlayer())
        {
            shooter.SetLastKnownPlayerPosition(
                player.position
            );
        }
        else
        {
            enemy.StateMachine.ChangeState(
                new ShooterSearchState(
                    enemy
                )
            );

            return;
        }

        if (shooter.IsAttackAnimationPlaying())
        {
            shooter.StopMoving();

            LookAtPlayer(
                player
            );

            return;
        }

        Vector3 direction =
            player.position -
            shooter.transform.position;

        direction.y =
            0f;


        float distance =
            direction.magnitude;

        if (
            distance <
            MinAttackDistance
        )
        {
            MoveAwayFromPlayer(
                player
            );

            return;
        }

        shooter.StopMoving();

        LookAtPlayer(
            player
        );

        attackTimer -=
            Time.deltaTime;


        if (
            attackTimer <=
            0f
        )
        {
            StartAttack(
                player
            );

            attackCount++;


            attackTimer =
                shooter.AttackCooldown;

            if (
                attackCount >=
                AttacksBeforeBackstep
            )
            {
                enemy.StateMachine.ChangeState(
                    new ShooterBackstepState(
                        enemy
                    )
                );

                return;
            }
        }
    }

    private void StartAttack(
        Transform player
    )
    {
        if (
            shooter.IsAttackAnimationPlaying()
        )
        {
            return;
        }

        Vector3 aimDirection =
            CalculateAimDirection(
                player
            );


        if (
            aimDirection.sqrMagnitude <
            0.001f
        )
        {
            return;
        }

        shooter.QueueProjectile(
            aimDirection
        );

        shooter.StartAttackAnimation();

        if (
            enemySoundController != null
        )
        {
            enemySoundController.PlayAttack();
        }
    }

    private Vector3 CalculateAimDirection(
        Transform player
    )
    {
        if (player == null)
            return Vector3.zero;

        Vector3 shooterPosition =
            shooter.SpitOrigin.position;

        PlayerAimTarget aimTarget =
            player.GetComponent<
                PlayerAimTarget
            >();


        Vector3 playerPosition =
            aimTarget != null &&
            aimTarget.AimPoint != null
                ? aimTarget.AimPoint.position
                : player.position;

        Vector3 playerVelocity =
            GetPlayerVelocity(
                player
            );

        float projectileSpeed =
            Mathf.Max(
                shooter.ProjectileSpeed,
                0.01f
            );

        float interceptTime =
            CalculateInterceptTime(
                shooterPosition,
                playerPosition,
                playerVelocity,
                projectileSpeed
            );

        Vector3 predictedPosition =
            playerPosition +
            playerVelocity *
            interceptTime;

        Vector3 aimDirection =
            predictedPosition -
            shooterPosition;


        if (
            aimDirection.sqrMagnitude <
            0.001f
        )
        {
            return Vector3.zero;
        }


        aimDirection.Normalize();

        aimDirection =
            ApplySpread(
                aimDirection,
                shooter.AimSpread
            );


        return aimDirection;
    }

    private float CalculateInterceptTime(
        Vector3 shooterPosition,
        Vector3 targetPosition,
        Vector3 targetVelocity,
        float projectileSpeed
    )
    {
        Vector3 relativePosition =
            targetPosition -
            shooterPosition;


        float a =
            targetVelocity.sqrMagnitude -
            projectileSpeed *
            projectileSpeed;


        float b =
            2f *
            Vector3.Dot(
                relativePosition,
                targetVelocity
            );


        float c =
            relativePosition.sqrMagnitude;


        if (
            Mathf.Abs(a) <
            0.001f
        )
        {
            if (
                Mathf.Abs(b) <
                0.001f
            )
            {
                return
                    relativePosition.magnitude /
                    projectileSpeed;
            }


            float interceptTime =
                -c / b;


            return Mathf.Max(
                interceptTime,
                0f
            );
        }


        float discriminant =
            b * b -
            4f * a * c;


        if (discriminant < 0f)
        {
            return
                relativePosition.magnitude /
                projectileSpeed;
        }


        float sqrt =
            Mathf.Sqrt(
                discriminant
            );


        float time1 =
            (-b + sqrt) /
            (2f * a);


        float time2 =
            (-b - sqrt) /
            (2f * a);


        float time =
            Mathf.Min(
                time1 > 0f
                    ? time1
                    : float.MaxValue,

                time2 > 0f
                    ? time2
                    : float.MaxValue
            );


        if (
            time ==
            float.MaxValue
        )
        {
            return
                relativePosition.magnitude /
                projectileSpeed;
        }


        return time;
    }

    private Vector3 GetPlayerVelocity(
        Transform player
    )
    {
        CharacterController controller =
            player.GetComponent<
                CharacterController
            >();


        if (controller != null)
        {
            return
                controller.velocity;
        }


        Rigidbody rigidbody =
            player.GetComponent<
                Rigidbody
            >();


        if (rigidbody != null)
        {
            return
                rigidbody.velocity;
        }


        return
            Vector3.zero;
    }

    private Vector3 ApplySpread(
        Vector3 direction,
        float spreadAngle
    )
    {
        if (
            spreadAngle <=
            0f
        )
        {
            return direction;
        }


        Quaternion spreadRotation =
            Quaternion.Euler(
                Random.Range(
                    -spreadAngle,
                    spreadAngle
                ),

                Random.Range(
                    -spreadAngle,
                    spreadAngle
                ),

                0f
            );


        return
            spreadRotation *
            direction;
    }

    private void MoveAwayFromPlayer(
        Transform player
    )
    {
        if (
            shooter.Agent == null ||
            !shooter.Agent.isOnNavMesh
        )
        {
            return;
        }


        Vector3 direction =
            shooter.transform.position -
            player.position;

        direction.y =
            0f;


        if (
            direction.sqrMagnitude <
            0.001f
        )
        {
            return;
        }


        direction.Normalize();


        Vector3 targetPosition =
            shooter.transform.position +
            direction * 3f;


        shooter.Agent.isStopped =
            false;

        shooter.Agent.SetDestination(
            targetPosition
        );


        LookAtPlayer(
            player
        );
    }

    private void LookAtPlayer(
        Transform player
    )
    {
        if (player == null)
            return;


        Vector3 direction =
            player.position -
            shooter.transform.position;

        direction.y =
            0f;


        if (
            direction.sqrMagnitude <
            0.001f
        )
        {
            return;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );


        shooter.transform.rotation =
            Quaternion.Slerp(
                shooter.transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
            );
    }

    public override void Exit()
    {
        if (
            shooter != null
        )
        {
            shooter.StopMoving();

            shooter.CancelAttackAnimation();
        }
    }
}