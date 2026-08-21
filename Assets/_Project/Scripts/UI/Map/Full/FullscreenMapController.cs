using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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


    private TPSInputActions inputActions;


    private readonly Dictionary<
        FullscreenMapMarker,
        FullscreenMapMarkerUI
    > markerUIs = new();


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        inputActions = new TPSInputActions();


        if (fullscreenMap != null)
        {
            fullscreenMap.SetActive(false);
        }


        if (fullscreenMapCamera != null)
        {
            fullscreenMapCamera.gameObject.SetActive(false);
        }
    }


    private void OnEnable()
    {
        inputActions.UI.Map.performed += OnMapPressed;

        inputActions.UI.Enable();
    }


    private void OnDisable()
    {
        inputActions.UI.Map.performed -= OnMapPressed;

        inputActions.UI.Disable();
    }


    private void OnDestroy()
    {
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
        if (fullscreenMap != null)
        {
            fullscreenMap.SetActive(true);
        }


        if (fullscreenMapCamera != null)
        {
            fullscreenMapCamera.gameObject.SetActive(true);
        }


        RefreshMarkers();
        UpdateMarkers();
    }


    public void CloseMap()
    {
        if (fullscreenMap != null)
        {
            fullscreenMap.SetActive(false);
        }


        if (fullscreenMapCamera != null)
        {
            fullscreenMapCamera.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // MARKERS
    // =========================================================

    private void RefreshMarkers()
    {
        FullscreenMapMarker[] markers =
            FindObjectsByType<FullscreenMapMarker>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );


        foreach (FullscreenMapMarker marker in markers)
        {
            if (marker == null)
                continue;


            if (markerUIs.ContainsKey(marker))
                continue;


            CreateMarker(marker);
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


        markerUI.Setup(marker.Icon);


        markerUIs.Add(
            marker,
            markerUI
        );
    }


    private void UpdateMarkers()
    {
        if (fullscreenMapCamera == null)
            return;


        RefreshMarkers();


        foreach (var pair in markerUIs)
        {
            FullscreenMapMarker marker =
                pair.Key;

            FullscreenMapMarkerUI markerUI =
                pair.Value;


            if (marker == null)
            {
                if (markerUI != null)
                {
                    Destroy(markerUI.gameObject);
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
            markerUI.SetVisible(false);
            return;
        }


        Vector3 viewportPosition =
            fullscreenMapCamera.WorldToViewportPoint(
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
            markerUI.SetVisible(false);
            return;
        }


        float x =
            (viewportPosition.x - 0.5f) *
            markersContainer.rect.width;


        float y =
            (viewportPosition.y - 0.5f) *
            markersContainer.rect.height;


        markerUI.RectTransform.anchoredPosition =
            new Vector2(x, y);


        markerUI.SetVisible(true);
    }
}