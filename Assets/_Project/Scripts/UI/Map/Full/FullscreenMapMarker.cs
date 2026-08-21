using UnityEngine;

public class FullscreenMapMarker : MonoBehaviour
{
    [Header("Marker")]
    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private bool visible = true;


    public Sprite Icon => icon;

    public bool IsVisible => visible;


    public void SetVisible(bool value)
    {
        visible = value;
    }
}