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

    [Header("Camera Recoil")]
    [SerializeField]
    private float recoilAmount = 0.6f;

    [SerializeField]
    private float recoilReturnSpeed = 10f;

    [SerializeField]
    private float recoilSnappiness = 20f;

    private IInputService inputService;

    private float horizontalAngle;
    private float verticalAngle;

    private bool wasAiming;

    private float currentRecoil;
    private float targetRecoil;

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
        UpdateRecoil();
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

        if (isAiming && !wasAiming)
        {
            EnterADS();
        }

        if (isAiming)
        {
            RotatePlayer(
                lookInput.x *
                currentHorizontalSensitivity
            );

            verticalAngle -=
                lookInput.y *
                currentVerticalSensitivity;
        }
        else
        {
            horizontalAngle +=
                lookInput.x *
                currentHorizontalSensitivity;

            verticalAngle -=
                lookInput.y *
                currentVerticalSensitivity;
        }

        verticalAngle =
            Mathf.Clamp(
                verticalAngle,
                minVerticalAngle,
                maxVerticalAngle
            );

        // Добавляем recoil поверх обычного вертикального взгляда.
        float recoilOffset =
            currentRecoil;

        cameraFollow.localRotation =
            Quaternion.Euler(
                verticalAngle - recoilOffset,
                isAiming ? 0f : horizontalAngle,
                0f
            );

        wasAiming = isAiming;
    }

    private void UpdateRecoil()
    {
        targetRecoil =
            Mathf.MoveTowards(
                targetRecoil,
                0f,
                recoilReturnSpeed *
                Time.deltaTime
            );

        currentRecoil =
            Mathf.Lerp(
                currentRecoil,
                targetRecoil,
                recoilSnappiness *
                Time.deltaTime
            );
    }

    public void AddRecoil()
    {
        targetRecoil += recoilAmount;

        targetRecoil =
            Mathf.Clamp(
                targetRecoil,
                0f,
                10f
            );
    }

    private void EnterADS()
    {
        if (playerTransform == null)
            return;

        float playerYaw =
            playerTransform.eulerAngles.y;

        float cameraWorldYaw =
            playerYaw + horizontalAngle;

        playerTransform.rotation =
            Quaternion.Euler(
                0f,
                cameraWorldYaw,
                0f
            );

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