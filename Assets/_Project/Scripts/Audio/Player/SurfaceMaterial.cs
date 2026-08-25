using UnityEngine;

[CreateAssetMenu(
    fileName = "SurfaceMaterial",
    menuName = "Game/Audio/Surface Material"
)]
public class SurfaceMaterial : ScriptableObject
{
    [Header("Surface Type")]
    [SerializeField]
    private SurfaceType surfaceType;

    public SurfaceType SurfaceType => surfaceType;


    [Header("Footstep Sounds")]
    public AudioClip[] footstepSounds;
}