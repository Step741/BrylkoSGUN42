using UnityEngine;

public class PickupFeedback : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField]
    private AudioClip pickupSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float volume = 1f;

    [SerializeField]
    private float soundPitch = 1f;


    [Header("Visual")]
    [SerializeField]
    private ParticleSystem pickupEffect;

    [SerializeField]
    private float effectLifetime = 2f;


    public void Play()
    {
        PlaySound();
        PlayEffect();
    }


    // =========================================================
    // AUDIO
    // =========================================================

    private void PlaySound()
    {
        if (pickupSound == null)
            return;

        GameObject audioObject =
            new GameObject("PickupAudio");

        audioObject.transform.position =
            transform.position;

        AudioSource audioSource =
            audioObject.AddComponent<AudioSource>();

        audioSource.clip = pickupSound;
        audioSource.volume = volume;
        audioSource.pitch = soundPitch;
        audioSource.spatialBlend = 0f;

        audioSource.Play();

        Destroy(
            audioObject,
            pickupSound.length + 0.1f
        );
    }


    // =========================================================
    // VISUAL EFFECT
    // =========================================================

    private void PlayEffect()
    {
        if (pickupEffect == null)
            return;

        ParticleSystem effect =
            Instantiate(
                pickupEffect,
                transform.position,
                Quaternion.identity
            );

        Destroy(
            effect.gameObject,
            effectLifetime
        );
    }
}