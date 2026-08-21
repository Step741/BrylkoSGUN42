using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform target;

    [Header("Settings")]
    [SerializeField]
    private float height = 20f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 position = target.position;
        position.y = height;

        transform.position = position;
    }
}