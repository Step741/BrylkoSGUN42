using UnityEngine;

[CreateAssetMenu(
    fileName = "UISoundLibrary",
    menuName = "Audio/UI Sound Library"
)]
public class UISoundLibrary : ScriptableObject
{
    [Header("Default UI Sounds")]

    [SerializeField]
    private AudioClip hoverSound;

    [SerializeField]
    private AudioClip clickSound;


    public AudioClip HoverSound => hoverSound;

    public AudioClip ClickSound => clickSound;
}