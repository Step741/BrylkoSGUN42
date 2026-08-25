using UnityEngine;

public class DoorSoundController : MonoBehaviour
{
    [Header("Door Sounds")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("Pitch")]
    [SerializeField]
    private Vector2 pitchRange =
        new Vector2(0.95f, 1.05f);

    [Header("3D Sound")]
    [SerializeField] private float minDistance = 2f;

    [SerializeField] private float maxDistance = 10f;

    public void PlayOpen()
    {
        PlaySound(openSound);
    }

    public void PlayClose()
    {
        PlaySound(closeSound);
    }

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