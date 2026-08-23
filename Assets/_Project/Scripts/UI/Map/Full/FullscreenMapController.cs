using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class FullscreenMapController : MonoBehaviour
{
    [Header("Map")]

    [SerializeField]
    private GameObject fullscreenMap;

    [SerializeField]
    private Camera fullscreenMapCamera;


    [Header("Markers")]

    [SerializeField]
    private RectTransform markersContainer;

    [SerializeField]
    private FullscreenMapMarkerUI markerPrefab;


    [Header("DOTween Animation")]

    [SerializeField]
    [Tooltip("Длительность открытия карты.")]
    private float openDuration = 0.25f;

    [SerializeField]
    [Tooltip("Длительность закрытия карты.")]
    private float closeDuration = 0.2f;

    [SerializeField]
    [Tooltip("Начальный масштаб карты при открытии.")]
    private float closedScale = 0.96f;

    [SerializeField]
    [Tooltip("Длительность появления нового маркера.")]
    private float markerAppearDuration = 0.2f;


    private TPSInputActions inputActions;


    private readonly Dictionary<
        FullscreenMapMarker,
        FullscreenMapMarkerUI
    > markerUIs = new();


    // =========================================================
    // DOTWEEN
    // =========================================================

    private CanvasGroup mapCanvasGroup;

    private Transform mapTransform;

    private Tween mapTween;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        inputActions =
            new TPSInputActions();


        if (fullscreenMap != null)
        {
            mapCanvasGroup =
                fullscreenMap.GetComponent<CanvasGroup>();


            mapTransform =
                fullscreenMap.transform;


            fullscreenMap.SetActive(false);
        }


        if (fullscreenMapCamera != null)
        {
            fullscreenMapCamera.gameObject.SetActive(
                false
            );
        }
    }


    private void OnEnable()
    {
        inputActions.UI.Map.performed +=
            OnMapPressed;

        inputActions.UI.Enable();
    }


    private void OnDisable()
    {
        inputActions.UI.Map.performed -=
            OnMapPressed;

        inputActions.UI.Disable();


        KillMapTween();
    }


    private void OnDestroy()
    {
        KillMapTween();

        inputActions.Dispose();
    }


    private void Update()
    {
        if (fullscreenMap == null)
            return;


        if (!fullscreenMap.activeSelf)
            return;


        UpdateMarkers();
    }


    // =========================================================
    // INPUT
    // =========================================================

    private void OnMapPressed(
        InputAction.CallbackContext context)
    {
        ToggleMap();
    }


    // =========================================================
    // MAP
    // =========================================================

    public void ToggleMap()
    {
        if (fullscreenMap == null)
            return;


        if (fullscreenMap.activeSelf)
        {
            CloseMap();
        }
        else
        {
            OpenMap();
        }
    }


    public void OpenMap()
    {
        if (fullscreenMap == null)
            return;


        // Останавливаем предыдущую анимацию.
        KillMapTween();


        // Включаем карту.
        fullscreenMap.SetActive(true);


        // Включаем камеру карты.
        if (fullscreenMapCamera != null)
        {
            fullscreenMapCamera.gameObject.SetActive(
                true
            );
        }


        // Обновляем маркеры сразу.
        RefreshMarkers();
        UpdateMarkers();


        // Если CanvasGroup отсутствует,
        // карта просто откроется как раньше.
        if (
            mapCanvasGroup == null ||
            mapTransform == null
        )
        {
            return;
        }


        // Начальное состояние анимации.
        mapCanvasGroup.alpha =
            0f;


        mapTransform.localScale =
            Vector3.one *
            closedScale;


        // Fade + Scale одновременно.
        Sequence sequence =
            DOTween.Sequence();


        sequence.Join(
            mapCanvasGroup
                .DOFade(
                    1f,
                    openDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
        );


        sequence.Join(
            mapTransform
                .DOScale(
                    Vector3.one,
                    openDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
        );


        mapTween =
            sequence;
    }


    public void CloseMap()
    {
        if (fullscreenMap == null)
            return;


        if (!fullscreenMap.activeSelf)
            return;


        // Останавливаем предыдущую анимацию.
        KillMapTween();


        // Если CanvasGroup отсутствует,
        // закрываем карту старым способом.
        if (
            mapCanvasGroup == null ||
            mapTransform == null
        )
        {
            CloseMapInstant();

            return;
        }


        Sequence sequence =
            DOTween.Sequence();


        // Плавно скрываем карту.
        sequence.Join(
            mapCanvasGroup
                .DOFade(
                    0f,
                    closeDuration
                )
                .SetEase(
                    Ease.InQuad
                )
        );


        // Слегка уменьшаем.
        sequence.Join(
            mapTransform
                .DOScale(
                    Vector3.one *
                    closedScale,
                    closeDuration
                )
                .SetEase(
                    Ease.InQuad
                )
        );


        // После завершения полностью выключаем карту.
        sequence.OnComplete(
            () =>
            {
                CloseMapInstant();
            }
        );


        mapTween =
            sequence;
    }


    private void CloseMapInstant()
    {
        if (fullscreenMap != null)
        {
            fullscreenMap.SetActive(
                false
            );
        }


        if (fullscreenMapCamera != null)
        {
            fullscreenMapCamera.gameObject.SetActive(
                false
            );
        }
    }


    // =========================================================
    // MARKERS
    // =========================================================

    private void RefreshMarkers()
    {
        FullscreenMapMarker[] markers =
            FindObjectsByType<
                FullscreenMapMarker
            >(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );


        foreach (
            FullscreenMapMarker marker
            in markers
        )
        {
            if (marker == null)
                continue;


            if (
                markerUIs.ContainsKey(
                    marker
                )
            )
            {
                continue;
            }


            CreateMarker(
                marker
            );
        }
    }


    private void CreateMarker(
        FullscreenMapMarker marker)
    {
        if (markerPrefab == null)
            return;


        if (markersContainer == null)
            return;


        FullscreenMapMarkerUI markerUI =
            Instantiate(
                markerPrefab,
                markersContainer
            );


        markerUI.Setup(
            marker.Icon
        );


        markerUIs.Add(
            marker,
            markerUI
        );


        // =====================================================
        // DOTWEEN MARKER APPEAR
        // =====================================================

        if (
            markerUI.RectTransform != null
        )
        {
            RectTransform markerTransform =
                markerUI.RectTransform;


            // На случай повторного использования.
            markerTransform.DOKill();


            markerTransform.localScale =
                Vector3.zero;


            markerTransform
                .DOScale(
                    Vector3.one,
                    markerAppearDuration
                )
                .SetEase(
                    Ease.OutBack
                );
        }
    }


    private void UpdateMarkers()
    {
        if (fullscreenMapCamera == null)
            return;


        RefreshMarkers();


        foreach (
            var pair
            in markerUIs
        )
        {
            FullscreenMapMarker marker =
                pair.Key;


            FullscreenMapMarkerUI markerUI =
                pair.Value;


            if (marker == null)
            {
                if (markerUI != null)
                {
                    markerUI.RectTransform.DOKill();

                    Destroy(
                        markerUI.gameObject
                    );
                }

                continue;
            }


            if (markerUI == null)
                continue;


            UpdateMarker(
                marker,
                markerUI
            );
        }
    }


    private void UpdateMarker(
        FullscreenMapMarker marker,
        FullscreenMapMarkerUI markerUI)
    {
        if (!marker.IsVisible)
        {
            markerUI.SetVisible(
                false
            );

            return;
        }


        Vector3 viewportPosition =
            fullscreenMapCamera
                .WorldToViewportPoint(
                    marker.transform.position
                );


        bool isInsideMap =
            viewportPosition.x >= 0f &&
            viewportPosition.x <= 1f &&
            viewportPosition.y >= 0f &&
            viewportPosition.y <= 1f &&
            viewportPosition.z > 0f;


        if (!isInsideMap)
        {
            markerUI.SetVisible(
                false
            );

            return;
        }


        float x =
            (
                viewportPosition.x -
                0.5f
            )
            *
            markersContainer.rect.width;


        float y =
            (
                viewportPosition.y -
                0.5f
            )
            *
            markersContainer.rect.height;


        // Позицию оставляем без DOTween.
        // Она обновляется каждый кадр.
        markerUI.RectTransform.anchoredPosition =
            new Vector2(
                x,
                y
            );


        markerUI.SetVisible(
            true
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillMapTween()
    {
        if (mapTween != null)
        {
            mapTween.Kill();

            mapTween =
                null;
        }


        if (mapCanvasGroup != null)
        {
            mapCanvasGroup.DOKill();
        }


        if (mapTransform != null)
        {
            mapTransform.DOKill();
        }
    }
}