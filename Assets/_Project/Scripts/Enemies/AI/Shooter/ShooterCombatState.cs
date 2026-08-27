using UnityEngine;


public class ShooterCombatState : EnemyState
{
    private readonly EnemyShooter shooter;

    private readonly EnemySoundController
        enemySoundController;


    private float attackTimer;

    private int attackCount;


    // ==========================================
    // ATTACK SETTINGS
    // ==========================================

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


    // ==========================================
    // ENTER
    // ==========================================

    public override void Enter()
    {
        if (shooter == null)
        {
            Debug.LogError(
                $"[{enemy.name}] ShooterCombatState: " +
                "EnemyShooter is missing."
            );

            return;
        }


        if (enemy.Vision == null)
        {
            Debug.LogError(
                $"[{enemy.name}] ShooterCombatState: " +
                "EnemyVision is missing."
            );

            return;
        }


        shooter.StopMoving();


        attackTimer =
            0f;

        attackCount =
            0;


        Debug.Log(
            $"[{enemy.name}] State: Combat"
        );
    }


    // ==========================================
    // TICK
    // ==========================================

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


        // ==========================================
        // LOW HEALTH -> TAKE COVER
        // ==========================================

        if (shooter.NeedsToTakeCover())
        {
            enemy.StateMachine.ChangeState(
                new ShooterTakeCoverState(
                    enemy
                )
            );

            return;
        }


        // ==========================================
        // PLAYER MEMORY / SEARCH
        // ==========================================

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


        // ==========================================
        // WHILE ATTACK ANIMATION IS PLAYING
        // ==========================================

        if (shooter.IsAttackAnimationPlaying())
        {
            shooter.StopMoving();

            LookAtPlayer(
                player
            );

            return;
        }


        // ==========================================
        // DISTANCE TO PLAYER
        // ==========================================

        Vector3 direction =
            player.position -
            shooter.transform.position;

        direction.y =
            0f;


        float distance =
            direction.magnitude;


        // ==========================================
        // TOO CLOSE
        // ==========================================

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


        // ==========================================
        // ATTACK POSITION
        // ==========================================

        shooter.StopMoving();

        LookAtPlayer(
            player
        );


        // ==========================================
        // ATTACK TIMER
        // ==========================================

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


            // ======================================
            // AFTER 3 ATTACKS -> BACKSTEP
            // ======================================

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


    // ==========================================
    // START ATTACK
    // ==========================================

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


        // ==========================================
        // CALCULATE PROJECTILE DIRECTION
        // ==========================================

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


        // ==========================================
        // QUEUE PROJECTILE
        // ==========================================

        shooter.QueueProjectile(
            aimDirection
        );


        // ==========================================
        // START ATTACK ANIMATION
        // ==========================================

        shooter.StartAttackAnimation();


        // ==========================================
        // ATTACK SOUND
        // ==========================================

        if (
            enemySoundController != null
        )
        {
            enemySoundController.PlayAttack();
        }
    }


    // ==========================================
    // CALCULATE AIM DIRECTION
    // ==========================================

    private Vector3 CalculateAimDirection(
        Transform player
    )
    {
        if (player == null)
            return Vector3.zero;


        // ==========================================
        // PROJECTILE ORIGIN
        //
        // Используем фактическую точку запуска.
        // ==========================================

        Vector3 shooterPosition =
            shooter.SpitOrigin.position;


        // ==========================================
        // PLAYER AIM POINT
        // ==========================================

        PlayerAimTarget aimTarget =
            player.GetComponent<
                PlayerAimTarget
            >();


        Vector3 playerPosition =
            aimTarget != null &&
            aimTarget.AimPoint != null
                ? aimTarget.AimPoint.position
                : player.position;


        // ==========================================
        // PLAYER VELOCITY
        // ==========================================

        Vector3 playerVelocity =
            GetPlayerVelocity(
                player
            );


        // ==========================================
        // PROJECTILE SPEED
        // ==========================================

        float projectileSpeed =
            Mathf.Max(
                shooter.ProjectileSpeed,
                0.01f
            );


        // ==========================================
        // INTERCEPT TIME
        // ==========================================

        float interceptTime =
            CalculateInterceptTime(
                shooterPosition,
                playerPosition,
                playerVelocity,
                projectileSpeed
            );


        // ==========================================
        // PREDICTED POSITION
        // ==========================================

        Vector3 predictedPosition =
            playerPosition +
            playerVelocity *
            interceptTime;


        // ==========================================
        // AIM DIRECTION
        // ==========================================

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


        // ==========================================
        // AIM SPREAD
        // ==========================================

        aimDirection =
            ApplySpread(
                aimDirection,
                shooter.AimSpread
            );


        return aimDirection;
    }


    // ==========================================
    // INTERCEPT TIME
    // ==========================================

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


    // ==========================================
    // PLAYER VELOCITY
    // ==========================================

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


    // ==========================================
    // SPREAD
    // ==========================================

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


    // ==========================================
    // MOVE AWAY FROM PLAYER
    // ==========================================

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


    // ==========================================
    // LOOK AT PLAYER
    // ==========================================

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


    // ==========================================
    // EXIT
    // ==========================================

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