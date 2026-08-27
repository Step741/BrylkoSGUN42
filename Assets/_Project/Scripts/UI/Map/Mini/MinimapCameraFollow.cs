using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Target")]

    [SerializeField]
    private Transform target;


    [Header("Settings")]

    [SerializeField]
    private float height = 20f;


    [Header("Rotation")]

    [SerializeField]
    private Vector3 fixedRotation =
        new Vector3(90f, 0f, 0f);


    // =========================================================
    // UNITY
    // =========================================================

    private void LateUpdate()
    {
        if (target == null)
            return;


        // =====================================================
        // POSITION
        // =====================================================

        Vector3 position =
            target.position;

        position.y =
            height;

        transform.position =
            position;


        // =====================================================
        // ROTATION
        // =====================================================

        // Миникарта всегда смотрит строго вниз
        // и не зависит от поворота игрока или основной камеры.
        transform.rotation =
            Quaternion.Euler(
                fixedRotation
            );
    }
}