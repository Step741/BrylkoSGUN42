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
    private PlayerVoiceController playerVoiceController;

    private float verticalVelocity;

    private float standingHeight;
    private Vector3 standingCenter;

    private bool isCrouching;
    private bool isSprinting;
    private bool isMoving;
    private bool isGrounded;


    // =========================
    // PUBLIC STATES
    // =========================

    public bool IsMoving => isMoving;

    public bool IsSprinting => isSprinting;

    public bool IsCrouching => isCrouching;

    public bool IsGrounded => isGrounded;


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

        playerVoiceController =
            GetComponent<PlayerVoiceController>();

        standingHeight =
            characterController.height;

        standingCenter =
            characterController.center;
    }


    private void Update()
    {
        // =========================
        // CROUCH
        // =========================

        UpdateCrouch();

        UpdateCharacterHeight();


        // =========================
        // MOVEMENT INPUT
        // =========================

        Vector3 movement =
            GetMovement();


        // Определяем движение сразу
        // по фактическому направлению,
        // а не через velocity после Move().
        isMoving =
            movement.sqrMagnitude > 0.0001f;


        // =========================
        // GROUND
        // =========================

        isGrounded =
            CheckGrounded();


        // =========================
        // GRAVITY & JUMP
        // =========================

        ApplyGravity();


        // =========================
        // SPRINT
        // =========================

        isSprinting =
            isMoving &&
            !isCrouching &&
            inputService.Sprint.IsPressed();


        // =========================
        // SPEED
        // =========================

        float currentSpeed;

        if (isCrouching)
        {
            currentSpeed =
                crouchSpeed;
        }
        else if (isSprinting)
        {
            currentSpeed =
                sprintSpeed;
        }
        else
        {
            currentSpeed =
                moveSpeed;
        }


        // =========================
        // MOVE
        // =========================

        Vector3 horizontalMovement =
            movement;

        movement *= currentSpeed;

        movement.y =
            verticalVelocity;


        characterController.Move(
            movement *
            Time.deltaTime
        );


        // =========================
        // VISUAL ROTATION
        // =========================

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
        if (
            isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity =
                -2f;
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

            if (playerVoiceController != null)
            {
                playerVoiceController.PlayJump();
            }
        }


        verticalVelocity +=
            gravity *
            Time.deltaTime;
    }


    private void UpdateCrouch()
    {
        if (
            inputService.Crouch.IsPressed())
        {
            isCrouching =
                true;

            return;
        }


        if (CanStandUp())
        {
            isCrouching =
                false;
        }
    }


    private void UpdateCharacterHeight()
    {
        float targetHeight =
            isCrouching
                ? crouchHeight
                : standingHeight;


        float newHeight =
            Mathf.MoveTowards(
                characterController.height,
                targetHeight,
                crouchTransitionSpeed *
                Time.deltaTime
            );


        characterController.height =
            newHeight;


        float bottomY =
            standingCenter.y -
            standingHeight * 0.5f;


        float targetCenterY =
            bottomY +
            newHeight * 0.5f;


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
        if (
            moveDirection.sqrMagnitude <
            0.01f)
        {
            return;
        }


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


        visual.localRotation =
            Quaternion.Slerp(
                visual.localRotation,
                Quaternion.identity,
                rotationSpeed *
                Time.deltaTime
            );
    }


    private bool CheckGrounded()
    {
        float sphereRadius =
            0.2f;


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


        float sphereRadius =
            0.2f;


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