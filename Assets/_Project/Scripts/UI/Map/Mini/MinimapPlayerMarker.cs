using UnityEngine;

public class MinimapPlayerMarker : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private RectTransform markerRect;


    [Header("Settings")]
    [SerializeField]
    private bool rotateWithView = true;

    [SerializeField]
    private float rotationOffset = 0f;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (markerRect == null)
        {
            markerRect =
                GetComponent<RectTransform>();
        }

        if (playerCamera == null)
        {
            playerCamera =
                Camera.main;
        }
    }


    private void LateUpdate()
    {
        if (markerRect == null)
            return;


        // =====================================================
        // POSITION
        // =====================================================

        // Игрок всегда находится в центре миникарты.
        markerRect.anchoredPosition =
            Vector2.zero;


        // =====================================================
        // ROTATION
        // =====================================================

        if (!rotateWithView)
            return;


        if (playerCamera == null)
            return;


        Vector3 forward =
            playerCamera.transform.forward;


        // Нам нужна только горизонтальная составляющая.
        forward.y = 0f;


        if (forward.sqrMagnitude < 0.001f)
            return;


        forward.Normalize();


        // Получаем угол направления взгляда
        // относительно мирового направления Z.
        float angle =
            Mathf.Atan2(
                forward.x,
                forward.z
            ) * Mathf.Rad2Deg;


        markerRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                -angle + rotationOffset
            );
    }
}