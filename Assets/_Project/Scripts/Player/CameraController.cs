using UnityEngine;
using Zenject;

public class CameraController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private Transform cameraFollow;

    [SerializeField]
    private Transform playerTransform;

    [Header("Sensitivity")]
    [SerializeField]
    private float horizontalSensitivity = 0.08f;

    [SerializeField]
    private float verticalSensitivity = 0.06f;

    [Header("ADS Sensitivity")]
    [SerializeField]
    private float adsSensitivityMultiplier = 0.5f;

    [Header("Vertical Limits")]
    [SerializeField]
    private float minVerticalAngle = -30f;

    [SerializeField]
    private float maxVerticalAngle = 60f;

    private IInputService inputService;

    private float horizontalAngle;
    private float verticalAngle;

    private bool wasAiming;

    [Inject]
    private void Construct(IInputService inputService)
    {
        this.inputService = inputService;
    }

    private void Start()
    {
        Vector3 currentRotation =
            cameraFollow.localEulerAngles;

        horizontalAngle =
            currentRotation.y;

        verticalAngle =
            NormalizeAngle(currentRotation.x);

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;
    }

    private void Update()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        Vector2 lookInput =
            inputService.Look.ReadValue<Vector2>();

        bool isAiming =
            inputService.Aim.IsPressed();

        float sensitivityMultiplier =
            isAiming
                ? adsSensitivityMultiplier
                : 1f;

        float currentHorizontalSensitivity =
            horizontalSensitivity *
            sensitivityMultiplier;

        float currentVerticalSensitivity =
            verticalSensitivity *
            sensitivityMultiplier;

        // Входим в ADS.
        // Переносим текущий горизонтальный угол
        // камеры на Player, чтобы камера не прыгнула.
        if (isAiming && !wasAiming)
        {
            EnterADS();
        }

        if (isAiming)
        {
            // В ADS горизонтальный поворот
            // выполняет сам Player.
            RotatePlayer(
                lookInput.x *
                currentHorizontalSensitivity
            );

            // Вертикаль по-прежнему принадлежит камере.
            verticalAngle -=
                lookInput.y *
                currentVerticalSensitivity;

            verticalAngle =
                Mathf.Clamp(
                    verticalAngle,
                    minVerticalAngle,
                    maxVerticalAngle
                );

            cameraFollow.localRotation =
                Quaternion.Euler(
                    verticalAngle,
                    0f,
                    0f
                );
        }
        else
        {
            // Обычный режим камеры.
            horizontalAngle +=
                lookInput.x *
                currentHorizontalSensitivity;

            verticalAngle -=
                lookInput.y *
                currentVerticalSensitivity;

            verticalAngle =
                Mathf.Clamp(
                    verticalAngle,
                    minVerticalAngle,
                    maxVerticalAngle
                );

            cameraFollow.localRotation =
                Quaternion.Euler(
                    verticalAngle,
                    horizontalAngle,
                    0f
                );
        }

        wasAiming = isAiming;
    }

    private void EnterADS()
    {
        if (playerTransform == null)
            return;

        // Получаем текущий мировой Y-угол камеры.
        float playerYaw =
            playerTransform.eulerAngles.y;

        float cameraWorldYaw =
            playerYaw + horizontalAngle;

        // Переносим направление камеры на Player.
        playerTransform.rotation =
            Quaternion.Euler(
                0f,
                cameraWorldYaw,
                0f
            );

        // Теперь CameraFollow смотрит прямо
        // относительно Player.
        horizontalAngle = 0f;
    }

    private void RotatePlayer(float rotation)
    {
        if (playerTransform == null)
            return;

        playerTransform.Rotate(
            Vector3.up,
            rotation,
            Space.World
        );
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}