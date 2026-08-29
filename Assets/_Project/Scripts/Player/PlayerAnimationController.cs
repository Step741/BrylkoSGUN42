using System.Collections;
using UnityEngine;
using Zenject;

public class PlayerAnimationController : MonoBehaviour
{
    private static readonly int MoveXHash =
        Animator.StringToHash("MoveX");

    private static readonly int MoveYHash =
        Animator.StringToHash("MoveY");

    private static readonly int CrouchHash =
        Animator.StringToHash("Crouch");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");

    private static readonly int HitReactionHash =
        Animator.StringToHash("Hit");

    private static readonly int ShootHash =
        Animator.StringToHash("Shoot");

    private static readonly int ShootRifleHash =
        Animator.StringToHash("ShootRifle");

    private static readonly int ShootShotgunHash =
        Animator.StringToHash("ShootShotgun");

    private static readonly int ShootGrenadeHash =
        Animator.StringToHash("ShootGrenade");

    private static readonly int ShootRailgunHash =
        Animator.StringToHash("ShootRailgun");

    private static readonly int KatanaAttackHash =
        Animator.StringToHash("KatanaAttack");

    private static readonly int ReloadHash =
        Animator.StringToHash("Reload");

    private static readonly int WeaponSwitchHash =
        Animator.StringToHash("WeaponSwitch");

    private static readonly int CombatReadyHash =
        Animator.StringToHash("CombatReady");

    [Header("References")]

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private Health health;

    [Header("Animation")]

    [SerializeField]
    private float smoothTime = 0.1f;

    [Header("Sprint Animation")]

    [SerializeField]
    private float sprintSpeedThreshold = 6.5f;

    [SerializeField]
    private float runBlendValue = 2f;

    [Header("Jump Animation")]

    [SerializeField]
    private float jumpAnimationDelay = 0.05f;

    [Header("Combat Ready")]

    [SerializeField]
    private float combatReadyDuration = 1.2f;

    private IInputService inputService;

    private Vector2 currentBlend;
    private Vector2 blendVelocity;

    private float jumpTimer;

    private float previousHealth;

    private Coroutine combatReadyCoroutine;

    [Inject]
    private void Construct(
        IInputService inputService)
    {
        this.inputService = inputService;
    }
    private void Awake()
    {
        if (health == null)
        {
            health =
                GetComponent<Health>();
        }

        if (characterController == null)
        {
            characterController =
                GetComponent<CharacterController>();
        }
    }


    private void OnEnable()
    {
        if (health != null)
        {
            health.HealthChanged +=
                OnHealthChanged;

            previousHealth =
                health.CurrentHealth;
        }
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -=
                OnHealthChanged;
        }

        StopCombatReady();
    }


    private void Update()
    {
        UpdateLocomotion();

        UpdateCrouchAnimation();

        UpdateJumpAnimation();
    }

    private void OnHealthChanged(
        float currentHealth,
        float maxHealth)
    {
        if (
            currentHealth < previousHealth &&
            currentHealth > 0f
        )
        {
            animator.SetTrigger(
                HitReactionHash
            );
        }

        previousHealth =
            currentHealth;
    }

    private void UpdateLocomotion()
    {
        Vector3 worldVelocity =
            characterController.velocity;

        worldVelocity.y = 0f;

        Vector3 localVelocity =
            transform.InverseTransformDirection(
                worldVelocity
            );

        float horizontalSpeed =
            localVelocity.magnitude;

        Vector2 targetBlend =
            Vector2.zero;

        if (horizontalSpeed > 0.01f)
        {
            Vector3 localDirection =
                localVelocity.normalized;

            targetBlend =
                new Vector2(
                    localDirection.x,
                    localDirection.z
                );

            Vector2 moveInput =
                inputService.Move
                    .ReadValue<Vector2>();

            bool isSprinting =
                inputService.Sprint
                    .IsPressed();

            bool sprintingForward =
                !inputService.Crouch.IsPressed() &&
                isSprinting &&
                moveInput.y > 0.5f &&
                horizontalSpeed >=
                    sprintSpeedThreshold;

            if (sprintingForward)
            {
                targetBlend.y =
                    runBlendValue;
            }
        }

        currentBlend =
            Vector2.SmoothDamp(
                currentBlend,
                targetBlend,
                ref blendVelocity,
                smoothTime
            );

        animator.SetFloat(
            MoveXHash,
            currentBlend.x
        );

        animator.SetFloat(
            MoveYHash,
            currentBlend.y
        );
    }

    private void UpdateCrouchAnimation()
    {
        bool isCrouching =
            inputService.Crouch.IsPressed();

        animator.SetBool(
            CrouchHash,
            isCrouching
        );
    }

    private void UpdateJumpAnimation()
    {
        if (!characterController.isGrounded)
        {
            jumpTimer +=
                Time.deltaTime;

            if (
                jumpTimer >=
                jumpAnimationDelay
            )
            {
                animator.SetBool(
                    JumpHash,
                    true
                );
            }

            return;
        }

        jumpTimer = 0f;

        animator.SetBool(
            JumpHash,
            false
        );
    }

    public void PlayShoot()
    {
        PlayShootAnimation(
            ShootHash
        );
    }


    public void PlayRifleShoot()
    {
        PlayShootAnimation(
            ShootRifleHash
        );
    }


    public void PlayShotgunShoot()
    {
        PlayShootAnimation(
            ShootShotgunHash
        );
    }


    public void PlayGrenadeShoot()
    {
        PlayShootAnimation(
            ShootGrenadeHash
        );
    }


    public void PlayRailgunShoot()
    {
        PlayShootAnimation(
            ShootRailgunHash
        );
    }


    private void PlayShootAnimation(
        int shootHash)
    {
        if (animator == null)
            return;

        if (combatReadyCoroutine != null)
        {
            StopCoroutine(
                combatReadyCoroutine
            );

            combatReadyCoroutine = null;
        }

        ResetShootTriggers();

        animator.SetBool(
            CombatReadyHash,
            true
        );

        animator.SetTrigger(
            shootHash
        );

        combatReadyCoroutine =
            StartCoroutine(
                CombatReadyTimer()
            );
    }


    private IEnumerator CombatReadyTimer()
    {
        yield return new WaitForSeconds(
            combatReadyDuration
        );

        ResetShootTriggers();

        animator.SetBool(
            CombatReadyHash,
            false
        );

        combatReadyCoroutine = null;
    }

    private void ResetShootTriggers()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(
            ShootHash
        );

        animator.ResetTrigger(
            ShootRifleHash
        );

        animator.ResetTrigger(
            ShootShotgunHash
        );

        animator.ResetTrigger(
            ShootGrenadeHash
        );

        animator.ResetTrigger(
            ShootRailgunHash
        );
    }

    private void StopCombatReady()
    {
        if (combatReadyCoroutine != null)
        {
            StopCoroutine(
                combatReadyCoroutine
            );

            combatReadyCoroutine = null;
        }

        if (animator != null)
        {

            ResetShootTriggers();

            animator.SetBool(
                CombatReadyHash,
                false
            );
        }
    }

    public void PlayKatanaAttack()
    {
        StopCombatReady();

        animator.SetTrigger(
            KatanaAttackHash
        );
    }

    public void PlayReload()
    {
        StopCombatReady();

        animator.SetTrigger(
            ReloadHash
        );
    }

    public void PlayWeaponSwitch()
    {
        if (animator == null)
            return;

        StopCombatReady();

        animator.SetTrigger(
            WeaponSwitchHash
        );
    }
}