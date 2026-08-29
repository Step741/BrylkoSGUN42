using UnityEngine;

public class MinimapMarker : MonoBehaviour
{
    public enum MarkerType
    {
        Enemy,
        Door,
        KeyCard,
        Health,
        Weapon,
        MissionTarget,
        Exit
    }


    [Header("Marker")]
    [SerializeField]
    private MarkerType markerType;

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private bool showOutsideRadius = false;

    [SerializeField]
    private bool clampToEdge = false;


    [Header("Visibility")]
    [SerializeField]
    private bool visible = true;


    public MarkerType Type =>
        markerType;

    public Sprite Icon =>
        icon;

    public bool ShowOutsideRadius =>
        showOutsideRadius;

    public bool ClampToEdge =>
        clampToEdge;

    public bool Visible =>
        visible;

    private void OnEnable()
    {
        TryRegister();
    }


    private void Start()
    {
        TryRegister();
    }


    private void OnDisable()
    {
        MinimapController.Unregister(this);
    }

    public void SetVisible(bool value)
    {
        visible = value;
    }

    private void TryRegister()
    {
        MinimapController.Register(this);
    }
}