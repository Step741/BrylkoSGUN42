using System.Collections.Generic;
using UnityEngine;

public class MinimapMarkerPool : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField]
    private GameObject markerPrefab;

    [SerializeField]
    private int initialSize = 16;

    [Header("UI Container")]
    [SerializeField]
    private RectTransform markerContainer;


    private readonly Queue<GameObject> pool =
        new Queue<GameObject>();


    private void Awake()
    {
        if (markerPrefab == null)
        {
            Debug.LogError(
                "[MinimapMarkerPool] Marker Prefab is not assigned.",
                this
            );

            return;
        }

        if (markerContainer == null)
        {
            Debug.LogError(
                "[MinimapMarkerPool] Marker Container is not assigned.",
                this
            );

            return;
        }


        for (int i = 0; i < initialSize; i++)
        {
            CreateMarker();
        }
    }


    private GameObject CreateMarker()
    {
        GameObject marker =
            Instantiate(
                markerPrefab,
                markerContainer
            );


        marker.SetActive(false);

        pool.Enqueue(marker);

        return marker;
    }


    public GameObject Get()
    {
        if (pool.Count == 0)
        {
            CreateMarker();
        }


        GameObject marker =
            pool.Dequeue();


        marker.transform.SetParent(
            markerContainer,
            false
        );


        marker.SetActive(true);

        return marker;
    }


    public void Release(GameObject marker)
    {
        if (marker == null)
            return;


        marker.SetActive(false);


        marker.transform.SetParent(
            markerContainer,
            false
        );


        RectTransform rect =
            marker.GetComponent<RectTransform>();


        if (rect != null)
        {
            rect.anchoredPosition =
                Vector2.zero;

            rect.localRotation =
                Quaternion.identity;

            rect.localScale =
                Vector3.one;
        }


        pool.Enqueue(marker);
    }
}