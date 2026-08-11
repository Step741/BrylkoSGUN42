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

    private IInputService inputService;

    private Vector2 currentBlend;
    private Vector2 blendVelocity;

    private float jumpTimer;

    private float previousHealth;

    [Inject]
    private void Construct(IInputService inputService)
    {
        this.inputService = inputService;
    }

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
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
            health.HealthChanged += OnHealthChanged;
            previousHealth = health.CurrentHealth;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
        }
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
        // Если здоровье уменьшилось —
        // проигрываем Hit Reaction.
        //
        // При смерти реакцию урона не запускаем,
        // потому что должна проигрываться Death.
        if (currentHealth < previousHealth &&
            currentHealth > 0f)
        {
            animator.SetTrigger(HitReactionHash);
        }

        previousHealth = currentHealth;
    }

    private void UpdateLocomotion()
    {
        // Получаем фактическую скорость персонажа.
        Vector3 worldVelocity =
            characterController.velocity;

        // Вертикальная скорость не влияет
        // на выбор анимации движения.
        worldVelocity.y = 0f;

        // Переводим скорость в локальное пространство Player.
        Vector3 localVelocity =
            transform.InverseTransformDirection(
                worldVelocity
            );

        float horizontalSpeed =
            localVelocity.magnitude;

        Vector2 targetBlend = Vector2.zero;

        if (horizontalSpeed > 0.01f)
        {
            // Определяем направление движения.
            Vector3 localDirection =
                localVelocity.normalized;

            targetBlend = new Vector2(
                localDirection.x,
                localDirection.z
            );

            // Получаем реальный ввод игрока.
            Vector2 moveInput =
                inputService.Move.ReadValue<Vector2>();

            bool isSprinting =
                inputService.Sprint.IsPressed();

            // Sprint-анимация используется только при
            // движении вперёд с зажатым Shift.
            bool sprintingForward =
                !inputService.Crouch.IsPressed() &&
                isSprinting &&
                moveInput.y > 0.5f &&
                horizontalSpeed >= sprintSpeedThreshold;

            if (sprintingForward)
            {
                targetBlend.y = runBlendValue;
            }
        }

        // Плавно изменяем параметры Blend Tree.
        currentBlend = Vector2.SmoothDamp(
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
        // Если персонаж находится в воздухе,
        // запускаем Jump.
        if (!characterController.isGrounded)
        {
            jumpTimer += Time.deltaTime;

            if (jumpTimer >= jumpAnimationDelay)
            {
                animator.SetBool(
                    JumpHash,
                    true
                );
            }

            return;
        }

        // Персонаж снова на земле.
        jumpTimer = 0f;

        animator.SetBool(
            JumpHash,
            false
        );
    }
}