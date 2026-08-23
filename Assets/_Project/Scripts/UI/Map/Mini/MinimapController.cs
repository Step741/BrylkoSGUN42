using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    private static MinimapController instance;


    [Header("References")]

    [SerializeField]
    private Camera minimapCamera;

    [SerializeField]
    private RectTransform minimapRect;

    [SerializeField]
    private MinimapMarkerPool markerPool;


    [Header("Minimap Camera Settings")]

    [SerializeField]
    [Tooltip("Размер области обзора миникарты. Работает только для Orthographic Camera.")]
    [Min(0.1f)]
    private float minimapViewSize = 20f;


    private readonly List<MinimapMarker> markers =
        new List<MinimapMarker>();


    private readonly Dictionary<
        MinimapMarker,
        GameObject
    > activeMarkers =
        new Dictionary<
            MinimapMarker,
            GameObject
        >();


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        instance = this;

        ApplyCameraSettings();
    }


    private void OnValidate()
    {
        ApplyCameraSettings();
    }


    private void LateUpdate()
    {
        UpdateMarkers();
    }


    // =========================================================
    // CAMERA SETTINGS
    // =========================================================

    private void ApplyCameraSettings()
    {
        if (minimapCamera == null)
            return;


        if (!minimapCamera.orthographic)
        {
            return;
        }


        minimapCamera.orthographicSize =
            minimapViewSize;
    }


    // =========================================================
    // REGISTER
    // =========================================================

    public static void Register(
        MinimapMarker marker
    )
    {
        if (instance == null)
            return;

        if (marker == null)
            return;

        if (instance.markers.Contains(marker))
            return;


        instance.markers.Add(marker);


        GameObject uiMarker =
            instance.markerPool.Get();


        MinimapMarkerUI markerUI =
            uiMarker.GetComponent<MinimapMarkerUI>();


        if (markerUI != null)
        {
            markerUI.Setup(marker.Icon);
        }


        instance.activeMarkers.Add(
            marker,
            uiMarker
        );
    }


    public static void Unregister(
        MinimapMarker marker
    )
    {
        if (instance == null)
            return;

        if (marker == null)
            return;


        instance.markers.Remove(marker);


        if (
            instance.activeMarkers.TryGetValue(
                marker,
                out GameObject uiMarker
            )
        )
        {
            MinimapMarkerUI markerUI =
                uiMarker.GetComponent<MinimapMarkerUI>();


            if (markerUI != null)
            {
                markerUI.ResetMarker();
            }


            instance.markerPool.Release(
                uiMarker
            );


            instance.activeMarkers.Remove(
                marker
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void UpdateMarkers()
    {
        for (int i = markers.Count - 1; i >= 0; i--)
        {
            MinimapMarker marker =
                markers[i];


            if (marker == null)
            {
                markers.RemoveAt(i);
                continue;
            }


            if (
                !activeMarkers.TryGetValue(
                    marker,
                    out GameObject uiMarker
                )
            )
            {
                continue;
            }


            if (!marker.Visible)
            {
                uiMarker.SetActive(false);
                continue;
            }


            uiMarker.SetActive(true);


            UpdateMarkerPosition(
                marker,
                uiMarker
            );
        }
    }


    // =========================================================
    // POSITION
    // =========================================================

    private void UpdateMarkerPosition(
        MinimapMarker marker,
        GameObject uiMarker
    )
    {
        if (minimapCamera == null)
            return;

        if (minimapRect == null)
            return;


        RectTransform markerRect =
            uiMarker.GetComponent<RectTransform>();


        if (markerRect == null)
            return;


        MinimapMarkerUI markerUI =
            uiMarker.GetComponent<MinimapMarkerUI>();


        Vector3 viewportPosition =
            minimapCamera.WorldToViewportPoint(
                marker.transform.position
            );


        // =====================================================
        // OBJECT BEHIND CAMERA
        // =====================================================

        if (viewportPosition.z < 0f)
        {
            // Если объект нельзя показывать за пределами миникарты,
            // сразу скрываем маркер.
            if (!marker.ShowOutsideRadius)
            {
                uiMarker.SetActive(false);
                return;
            }


            // Показывать за пределами можно только через Clamp.
            if (!marker.ClampToEdge)
            {
                uiMarker.SetActive(false);
                return;
            }


            // Инвертируем направление,
            // чтобы определить правильную сторону края.
            viewportPosition.x =
                1f - viewportPosition.x;

            viewportPosition.y =
                1f - viewportPosition.y;
        }


        // =====================================================
        // CHECK OUTSIDE
        // =====================================================

        bool outside =
            viewportPosition.x < 0f ||
            viewportPosition.x > 1f ||
            viewportPosition.y < 0f ||
            viewportPosition.y > 1f;


        // =====================================================
        // OBJECT OUTSIDE MINIMAP
        // =====================================================

        if (outside)
        {
            // ShowOutsideRadius определяет,
            // разрешено ли вообще показывать объект вне области.
            if (!marker.ShowOutsideRadius)
            {
                uiMarker.SetActive(false);
                return;
            }


            // Если показывать разрешено, но Clamp выключен,
            // не выводим UI за пределы миникарты.
            if (!marker.ClampToEdge)
            {
                uiMarker.SetActive(false);
                return;
            }


            // -------------------------------------------------
            // DIRECTION FROM CENTER
            // -------------------------------------------------

            Vector2 direction =
                new Vector2(
                    viewportPosition.x - 0.5f,
                    viewportPosition.y - 0.5f
                );


            if (direction.sqrMagnitude < 0.001f)
            {
                direction =
                    Vector2.up;
            }
            else
            {
                direction.Normalize();
            }


            // -------------------------------------------------
            // POSITION ON EDGE
            // -------------------------------------------------

            float halfWidth =
                minimapRect.rect.width * 0.5f - 14f;

            float halfHeight =
                minimapRect.rect.height * 0.5f - 14f;


            float scaleX =
                Mathf.Abs(direction.x) > 0.001f
                    ? halfWidth / Mathf.Abs(direction.x)
                    : float.MaxValue;


            float scaleY =
                Mathf.Abs(direction.y) > 0.001f
                    ? halfHeight / Mathf.Abs(direction.y)
                    : float.MaxValue;


            float scale =
                Mathf.Min(
                    scaleX,
                    scaleY
                );


            markerRect.anchoredPosition =
                direction * scale;


            // -------------------------------------------------
            // SHOW EDGE ARROW
            // -------------------------------------------------

            if (markerUI != null)
            {
                markerUI.ShowEdgeArrow(direction);
            }


            uiMarker.SetActive(true);

            return;
        }


        // =====================================================
        // OBJECT INSIDE MINIMAP
        // =====================================================

        float x =
            (viewportPosition.x - 0.5f) *
            minimapRect.rect.width;


        float y =
            (viewportPosition.y - 0.5f) *
            minimapRect.rect.height;


        markerRect.anchoredPosition =
            new Vector2(
                x,
                y
            );


        // Показываем обычную иконку.
        if (markerUI != null)
        {
            markerUI.ShowInside();
        }


        uiMarker.SetActive(true);
    }
}