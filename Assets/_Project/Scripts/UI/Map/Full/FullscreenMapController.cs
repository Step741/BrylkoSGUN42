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
    private float openDuration = 0.25f;

    [SerializeField]
    private float closeDuration = 0.2f;

    [SerializeField]
    private float closedScale = 0.96f;

    [SerializeField]
    private float markerAppearDuration = 0.2f;


    private TPSInputActions inputActions;


    private readonly Dictionary<
        FullscreenMapMarker,
        FullscreenMapMarkerUI
    > markerUIs = new();

    private CanvasGroup mapCanvasGroup;

    private Transform mapTransform;

    private Tween mapTween;

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


        //Останавливает все активные анимации
        KillAllTweens();
    }


    private void OnDestroy()
    {
        KillAllTweens();


        if (inputActions != null)
        {
            inputActions.Dispose();

            inputActions = null;
        }
    }


    private void Update()
    {
        if (fullscreenMap == null)
            return;


        if (!fullscreenMap.activeSelf)
            return;


        UpdateMarkers();
    }

    private void OnMapPressed(
        InputAction.CallbackContext context)
    {
        ToggleMap();
    }

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

        KillMapTween();

        fullscreenMap.SetActive(true);

        if (fullscreenMapCamera != null)
        {
            fullscreenMapCamera.gameObject.SetActive(
                true
            );
        }

        RefreshMarkers();

        UpdateMarkers();

        if (
            mapCanvasGroup == null ||
            mapTransform == null
        )
        {
            return;
        }

        mapCanvasGroup.alpha =
            0f;


        mapTransform.localScale =
            Vector3.one *
            closedScale;

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

        sequence.SetLink(
            fullscreenMap
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

        KillMapTween();

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

        sequence.OnComplete(
            () =>
            {
                if (
                    fullscreenMap != null
                )
                {
                    CloseMapInstant();
                }
            }
        );

        sequence.SetLink(
            fullscreenMap
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

        if (
            markerUI.RectTransform != null
        )
        {
            RectTransform markerTransform =
                markerUI.RectTransform;

            markerTransform.DOKill();


            markerTransform.localScale =
                Vector3.zero;


            Tween markerTween =
                markerTransform
                    .DOScale(
                        Vector3.one,
                        markerAppearDuration
                    )
                    .SetEase(
                        Ease.OutBack
                    );

            markerTween.SetLink(
                markerUI.gameObject
            );
        }
    }


    private void UpdateMarkers()
    {
        if (fullscreenMapCamera == null)
            return;


        RefreshMarkers();


        List<FullscreenMapMarker> markersToRemove =
            new();


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
                    if (
                        markerUI.RectTransform != null
                    )
                    {
                        markerUI.RectTransform.DOKill();
                    }


                    Destroy(
                        markerUI.gameObject
                    );
                }


                markersToRemove.Add(
                    marker
                );


                continue;
            }

            if (markerUI == null)
            {
                markersToRemove.Add(
                    marker
                );

                continue;
            }


            UpdateMarker(
                marker,
                markerUI
            );
        }

        foreach (
            FullscreenMapMarker marker
            in markersToRemove
        )
        {
            markerUIs.Remove(
                marker
            );
        }
    }


    private void UpdateMarker(
        FullscreenMapMarker marker,
        FullscreenMapMarkerUI markerUI)
    {
        if (
            marker == null ||
            markerUI == null
        )
        {
            return;
        }


        if (
            markerUI.RectTransform == null
        )
        {
            return;
        }


        if (!marker.IsVisible)
        {
            markerUI.SetVisible(
                false
            );

            return;
        }


        if (fullscreenMapCamera == null)
            return;


        if (markersContainer == null)
            return;


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

        markerUI.RectTransform.anchoredPosition =
            new Vector2(
                x,
                y
            );


        markerUI.SetVisible(
            true
        );
    }

    //CLEANUP
    private void KillAllTweens()
    {
        KillMapTween();

        KillMarkerTweens();
    }


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


    private void KillMarkerTweens()
    {
        foreach (
            var pair
            in markerUIs
        )
        {
            FullscreenMapMarkerUI markerUI =
                pair.Value;


            if (markerUI == null)
                continue;


            RectTransform markerTransform =
                markerUI.RectTransform;


            if (markerTransform != null)
            {
                markerTransform.DOKill();
            }
        }


        markerUIs.Clear();
    }
}