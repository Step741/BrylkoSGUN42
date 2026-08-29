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

    [Header("Free Fire Rotation")]

    [SerializeField]
    private float freeFireRotationSpeed = 720f;

    [SerializeField]
    private float animationAimOffset = 18f;


    [Header("ADS Rotation")]

    [SerializeField]
    private float adsAnimationAimOffset = 15f;

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

    public bool IsMoving =>
        isMoving;

    public bool IsSprinting =>
        isSprinting;

    public bool IsCrouching =>
        isCrouching;

    public bool IsGrounded =>
        isGrounded;

    [Inject]
    private void Construct(
        IInputService inputService,
        Camera mainCamera)
    {
        this.inputService =
            inputService;

        cameraTransform =
            mainCamera.transform;
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
        UpdateCrouch();

        UpdateCharacterHeight();

        Vector3 movement =
            GetMovement();


        isMoving =
            movement.sqrMagnitude >
            0.0001f;

        isGrounded =
            CheckGrounded();

        ApplyGravity();

        isSprinting =
            isMoving &&
            !isCrouching &&
            inputService.Sprint.IsPressed();

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

        Vector3 horizontalMovement =
            movement;


        movement *=
            currentSpeed;


        movement.y =
            verticalVelocity;


        characterController.Move(
            movement *
            Time.deltaTime
        );

        if (inputService.Aim.IsPressed())
        {
            AlignVisualWithPlayer();
        }
        else if (
            inputService.Fire.IsPressed()
        )
        {
            RotateVisualTowardsAim();
        }
        else
        {
            RotateVisual(
                horizontalMovement
            );
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


        cameraForward.y =
            0f;

        cameraRight.y =
            0f;


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
            verticalVelocity < 0f
        )
        {
            verticalVelocity =
                -2f;
        }


        if (
            inputService.Jump.WasPressedThisFrame() &&
            isGrounded
        )
        {
            verticalVelocity =
                Mathf.Sqrt(
                    jumpHeight *
                    -2f *
                    gravity
                );


            if (
                playerVoiceController !=
                null
            )
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
            inputService.Crouch.IsPressed()
        )
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
        if (visual == null)
            return;


        if (
            moveDirection.sqrMagnitude <
            0.01f
        )
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

    private void RotateVisualTowardsAim()
    {
        if (visual == null)
            return;


        if (cameraTransform == null)
            return;


        //Направление взгляда камеры
        Vector3 aimDirection =
            cameraTransform.forward;


        //Убирает вертикальную составляющую
        aimDirection.y =
            0f;


        if (
            aimDirection.sqrMagnitude <
            0.001f
        )
        {
            return;
        }


        aimDirection.Normalize();


        //Компенсация смещения обычной анимации стрельбы
        Quaternion aimRotation =
            Quaternion.LookRotation(
                aimDirection
            );


        aimRotation *=
            Quaternion.Euler(
                0f,
                animationAimOffset,
                0f
            );

        visual.rotation =
            Quaternion.RotateTowards(
                visual.rotation,
                aimRotation,
                freeFireRotationSpeed *
                Time.deltaTime
            );
    }

    private void AlignVisualWithPlayer()
    {
        if (visual == null)
            return;

        Quaternion targetRotation =
            transform.rotation *
            Quaternion.Euler(
                0f,
                adsAnimationAimOffset,
                0f
            );


        visual.rotation =
            Quaternion.Slerp(
                visual.rotation,
                targetRotation,
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


    //GIZMOS
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