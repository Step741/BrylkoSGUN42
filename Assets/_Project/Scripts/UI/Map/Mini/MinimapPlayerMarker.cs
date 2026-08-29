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

    [SerializeField]
    private RectTransform rotationTarget;


    [Header("Settings")]

    [SerializeField]
    private bool rotateWithView = true;

    [SerializeField]
    private float rotationOffset = 0f;

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

        if (rotationTarget == null)
        {
            rotationTarget =
                markerRect;
        }

        SetupMarkerPosition();
    }


    private void LateUpdate()
    {
        if (markerRect == null)
            return;

        //Игрок всегда находится точно в центре миникарты
        markerRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        markerRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        markerRect.pivot =
            new Vector2(0.5f, 0.5f);

        markerRect.anchoredPosition =
            Vector2.zero;

        if (!rotateWithView)
            return;

        if (playerCamera == null)
            return;

        if (rotationTarget == null)
            return;


        Vector3 forward =
            playerCamera.transform.forward;

        //Убирает наклон камеры
        forward.y = 0f;


        if (forward.sqrMagnitude < 0.001f)
            return;


        forward.Normalize();

        float angle =
            Mathf.Atan2(
                forward.x,
                forward.z
            ) * Mathf.Rad2Deg;

        rotationTarget.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                -angle + rotationOffset
            );
    }

    private void SetupMarkerPosition()
    {
        if (markerRect == null)
            return;


        markerRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        markerRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        markerRect.pivot =
            new Vector2(0.5f, 0.5f);

        markerRect.anchoredPosition =
            Vector2.zero;
    }
}