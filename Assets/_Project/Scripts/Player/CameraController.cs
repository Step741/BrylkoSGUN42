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

    private const string MouseSensitivityKey =
        "MouseSensitivity";

    private const string InvertYAxisKey =
        "InvertYAxis";

    private const float MinMouseSensitivity =
        0.1f;

    private const float MaxMouseSensitivity =
        5f;

    private const float DefaultMouseSensitivity =
        2f;


    private float settingsSensitivity =
        DefaultMouseSensitivity;

    private bool invertYAxis;

    private IInputService inputService;

    private float horizontalAngle;
    private float verticalAngle;

    private bool wasAiming;

    private float currentRecoil;
    private float targetRecoil;


    [Inject]
    private void Construct(
        IInputService inputService)
    {
        this.inputService = inputService;
    }


    private void Start()
    {
        LoadSettings();


        Vector3 currentRotation =
            cameraFollow.localEulerAngles;


        horizontalAngle =
            currentRotation.y;


        verticalAngle =
            NormalizeAngle(
                currentRotation.x
            );


        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible =
            false;
    }


    private void Update()
    {
        UpdateSettings();

        RotateCamera();

        UpdateRecoil();
    }

    private void LoadSettings()
    {
        settingsSensitivity =
            Mathf.Clamp(
                PlayerPrefs.GetFloat(
                    MouseSensitivityKey,
                    DefaultMouseSensitivity
                ),
                MinMouseSensitivity,
                MaxMouseSensitivity
            );


        PlayerPrefs.SetFloat(
            MouseSensitivityKey,
            settingsSensitivity
        );


        PlayerPrefs.Save();


        invertYAxis =
            PlayerPrefs.GetInt(
                InvertYAxisKey,
                0
            ) == 1;
    }


    private void UpdateSettings()
    {
        settingsSensitivity =
            Mathf.Clamp(
                PlayerPrefs.GetFloat(
                    MouseSensitivityKey,
                    DefaultMouseSensitivity
                ),
                MinMouseSensitivity,
                MaxMouseSensitivity
            );


        invertYAxis =
            PlayerPrefs.GetInt(
                InvertYAxisKey,
                0
            ) == 1;
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
            settingsSensitivity *
            sensitivityMultiplier;


        float currentVerticalSensitivity =
            verticalSensitivity *
            settingsSensitivity *
            sensitivityMultiplier;


        float verticalInputMultiplier =
            invertYAxis
                ? -1f
                : 1f;


        if (
            isAiming &&
            !wasAiming)
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
                currentVerticalSensitivity *
                verticalInputMultiplier;
        }
        else
        {
            horizontalAngle +=
                lookInput.x *
                currentHorizontalSensitivity;


            verticalAngle -=
                lookInput.y *
                currentVerticalSensitivity *
                verticalInputMultiplier;
        }


        verticalAngle =
            Mathf.Clamp(
                verticalAngle,
                minVerticalAngle,
                maxVerticalAngle
            );


        float recoilOffset =
            currentRecoil;


        cameraFollow.localRotation =
            Quaternion.Euler(
                verticalAngle -
                recoilOffset,

                isAiming
                    ? 0f
                    : horizontalAngle,

                0f
            );


        wasAiming =
            isAiming;
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
        targetRecoil +=
            recoilAmount;


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
            playerYaw +
            horizontalAngle;


        playerTransform.rotation =
            Quaternion.Euler(
                0f,
                cameraWorldYaw,
                0f
            );


        horizontalAngle =
            0f;
    }


    private void RotatePlayer(
        float rotation)
    {
        if (playerTransform == null)
            return;


        playerTransform.Rotate(
            Vector3.up,
            rotation,
            Space.World
        );
    }


    private float NormalizeAngle(
        float angle)
    {
        if (angle > 180f)
            angle -= 360f;


        return angle;
    }
}