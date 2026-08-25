using UnityEngine;

public class ElevatorSoundController : MonoBehaviour
{
    [Header("Movement Sounds")]

    [SerializeField]
    private AudioClip moveUpSound;

    [SerializeField]
    private AudioClip moveDownSound;


    [Header("Volume")]

    [SerializeField]
    [Range(0f, 1f)]
    private float volume = 1f;


    [Header("Pitch Randomization")]

    [SerializeField]
    private Vector2 pitchRange =
        new Vector2(0.98f, 1.02f);


    [Header("3D Sound Settings")]

    [SerializeField]
    private float minDistance = 3f;

    [SerializeField]
    private float maxDistance = 20f;


    // ==========================================
    // PUBLIC
    // ==========================================

    public void PlayMoveUp()
    {
        PlaySound(
            moveUpSound
        );
    }


    public void PlayMoveDown()
    {
        PlaySound(
            moveDownSound
        );
    }


    // ==========================================
    // PLAY SOUND
    // ==========================================

    private void PlaySound(
        AudioClip clip)
    {
        if (clip == null)
            return;

        if (SoundService.Instance == null)
            return;


        float pitch =
            Random.Range(
                pitchRange.x,
                pitchRange.y
            );


        SoundService.Instance.Play3D(
            clip,
            transform.position,
            SoundType.SFX,
            volume,
            pitch,
            minDistance,
            maxDistance
        );
    }
}