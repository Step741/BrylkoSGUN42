using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float sprintSpeed = 8f;

    [SerializeField]
    private float rotationSpeed = 10f;

    [SerializeField]
    private Transform visual;

    [Header("Jump & Gravity")]
    [SerializeField]
    private float gravity = -20f;

    [SerializeField]
    private float jumpHeight = 1.5f;

    [SerializeField]
    private float groundCheckDistance = 0.15f;

    [SerializeField]
    private LayerMask groundLayer;

    [Header("Crouch")]
    [SerializeField]
    private float crouchSpeed = 2.5f;

    [SerializeField]
    private float crouchHeight = 1.2f;

    [SerializeField]
    private float crouchTransitionSpeed = 10f;

    [SerializeField]
    private LayerMask obstacleLayer;

    private CharacterController characterController;
    private IInputService inputService;

    private Transform cameraTransform;

    private float verticalVelocity;

    private float standingHeight;
    private Vector3 standingCenter;

    private bool isCrouching;
    private bool isSprinting;

    [Inject]
    private void Construct(
        IInputService inputService,
        Camera mainCamera)
    {
        this.inputService = inputService;
        cameraTransform = mainCamera.transform;
    }

    private void Awake()
    {
        characterController =
            GetComponent<CharacterController>();

        // Запоминаем исходные параметры
        // стоящего персонажа.
        standingHeight =
            characterController.height;

        standingCenter =
            characterController.center;
    }

    private void Update()
    {
        // Обновляем состояние приседания.
        UpdateCrouch();

        // Плавно изменяем высоту CharacterController.
        UpdateCharacterHeight();

        // Получаем направление движения
        // относительно камеры.
        Vector3 movement =
            GetMovement();

        // Обрабатываем прыжок и гравитацию.
        ApplyGravity();

        // Sprint недоступен во время приседания.
        isSprinting =
            !isCrouching &&
            inputService.Sprint.IsPressed() &&
            movement.sqrMagnitude > 0.01f;

        float currentSpeed;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        movement *= currentSpeed;

        movement.y =
            verticalVelocity;

        characterController.Move(
            movement *
            Time.deltaTime
        );

        Vector3 horizontalMovement =
            movement;

        horizontalMovement.y = 0f;

        // В обычном режиме Visual смотрит
        // в сторону движения.
        //
        // В ADS Player уже смотрит туда же,
        // куда смотрит камера, поэтому Visual
        // не должен самостоятельно разворачиваться.
        if (!inputService.Aim.IsPressed())
        {
            RotateVisual(
                horizontalMovement
            );
        }
        else
        {
            AlignVisualWithPlayer();
        }
    }

    private Vector3 GetMovement()
    {
        Vector2 input =
            inputService.Move.ReadValue<Vector2>();

        Vector3 cameraForward =
            cameraTransform.forward;

        Vector3 cameraRight =
            cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection =
            cameraForward * input.y +
            cameraRight * input.x;

        return Vector3.ClampMagnitude(
            moveDirection,
            1f
        );
    }

    private void ApplyGravity()
    {
        bool isGrounded =
            IsGrounded();

        if (isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (
            inputService.Jump.WasPressedThisFrame() &&
            isGrounded)
        {
            verticalVelocity =
                Mathf.Sqrt(
                    jumpHeight *
                    -2f *
                    gravity
                );
        }

        verticalVelocity +=
            gravity *
            Time.deltaTime;
    }

    private void UpdateCrouch()
    {
        if (inputService.Crouch.IsPressed())
        {
            isCrouching = true;
            return;
        }

        if (CanStandUp())
        {
            isCrouching = false;
        }
    }

    private void UpdateCharacterHeight()
    {
        float targetHeight =
            isCrouching
                ? crouchHeight
                : standingHeight;

        // Плавно меняем высоту.
        float newHeight =
            Mathf.MoveTowards(
                characterController.height,
                targetHeight,
                crouchTransitionSpeed *
                Time.deltaTime
            );

        characterController.height =
            newHeight;

        // Сохраняем нижнюю точку капсулы.
        //
        // Исходная нижняя точка:
        // standingCenter.y - standingHeight / 2
        //
        // Поэтому при изменении высоты
        // центр автоматически смещается так,
        // чтобы низ капсулы оставался на месте.
        float bottomY =
            standingCenter.y -
            standingHeight *
            0.5f;

        float targetCenterY =
            bottomY +
            newHeight *
            0.5f;

        Vector3 center =
            characterController.center;

        center.y =
            targetCenterY;

        characterController.center =
            center;
    }

    private bool CanStandUp()
    {
        float radius =
            characterController.radius;

        Vector3 bottom =
            transform.position +
            Vector3.up *
            (radius + 0.01f);

        Vector3 top =
            transform.position +
            Vector3.up *
            (standingHeight - radius);

        return !Physics.CheckCapsule(
            bottom,
            top,
            radius,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void RotateVisual(
        Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude <
            0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                moveDirection
            );

        visual.rotation =
            Quaternion.Slerp(
                visual.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    private void AlignVisualWithPlayer()
    {
        if (visual == null)
            return;

        // Visual находится внутри Player,
        // поэтому локальный нулевой поворот
        // означает, что модель смотрит
        // в направлении Player.
        visual.localRotation =
            Quaternion.Slerp(
                visual.localRotation,
                Quaternion.identity,
                rotationSpeed *
                Time.deltaTime
            );
    }

    private bool IsGrounded()
    {
        float sphereRadius = 0.2f;

        Vector3 origin =
            transform.position +
            Vector3.up *
            (sphereRadius + 0.05f);

        float castDistance =
            sphereRadius +
            groundCheckDistance;

        return Physics.SphereCast(
            origin,
            sphereRadius,
            Vector3.down,
            out _,
            castDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        float sphereRadius = 0.2f;

        Vector3 origin =
            transform.position +
            Vector3.up *
            (sphereRadius + 0.05f);

        Gizmos.DrawWireSphere(
            origin,
            sphereRadius
        );

        Gizmos.DrawLine(
            origin,
            origin +
            Vector3.down *
            (
                sphereRadius +
                groundCheckDistance
            )
        );
    }
}