using UnityEngine;
using DG.Tweening;
using Cinemachine;
using Zenject;

public class ADSController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private Transform cameraFollow;

    [SerializeField]
    private Transform adsCameraTarget;

    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [Header("ADS Camera")]
    [SerializeField]
    private float adsMoveSpeed = 8f;

    [SerializeField]
    private float normalFov = 60f;

    [SerializeField]
    private float adsFov = 45f;

    [SerializeField]
    private float fovDuration = 0.2f;

    public bool IsAiming { get; private set; }

    private IInputService inputService;

    private Vector3 defaultCameraFollowPosition;

    private Tween fovTween;

    [Inject]
    private void Construct(IInputService inputService)
    {
        this.inputService = inputService;
    }

    private void Awake()
    {
        if (cameraFollow != null)
        {
            defaultCameraFollowPosition =
                cameraFollow.localPosition;
        }

        if (virtualCamera != null)
        {
            normalFov =
                virtualCamera.m_Lens.FieldOfView;
        }
    }

    private void Update()
    {
        UpdateAimState();
        UpdateCameraPosition();
    }

    private void UpdateAimState()
    {
        bool aiming =
            inputService.Aim.IsPressed();

        if (aiming == IsAiming)
            return;

        IsAiming = aiming;

        UpdateFov();
    }

    private void UpdateCameraPosition()
    {
        if (cameraFollow == null)
            return;

        Vector3 targetPosition =
            IsAiming && adsCameraTarget != null
                ? adsCameraTarget.localPosition
                : defaultCameraFollowPosition;

        cameraFollow.localPosition =
            Vector3.Lerp(
                cameraFollow.localPosition,
                targetPosition,
                adsMoveSpeed * Time.deltaTime
            );
    }

    private void UpdateFov()
    {
        if (virtualCamera == null)
            return;

        fovTween?.Kill();

        float targetFov =
            IsAiming
                ? adsFov
                : normalFov;

        fovTween =
            DOTween.To(
                () => virtualCamera.m_Lens.FieldOfView,
                value =>
                {
                    LensSettings lens =
                        virtualCamera.m_Lens;

                    lens.FieldOfView = value;

                    virtualCamera.m_Lens = lens;
                },
                targetFov,
                fovDuration
            )
            .SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        fovTween?.Kill();
    }
}