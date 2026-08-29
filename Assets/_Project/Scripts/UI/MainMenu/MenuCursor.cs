using UnityEngine;

public class MenuCursor : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField]
    private Texture2D cursorTexture;

    [SerializeField]
    private Vector2 hotspot = Vector2.zero;

    private void Awake()
    {
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible =
            true;

        Cursor.SetCursor(
            cursorTexture,
            hotspot,
            CursorMode.Auto
        );
    }

    private void OnDestroy()
    {
        //Возвращаеm системный курсор,для игровой сцены
        Cursor.SetCursor(
            null,
            Vector2.zero,
            CursorMode.Auto
        );
    }
}