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

    private void PlaySound()
    {
        if (pickupSound == null)
            return;

        if (SoundService.Instance == null)
            return;


        SoundService.Instance.Play2D(
            pickupSound,
            SoundType.SFX,
            volume,
            soundPitch
        );
    }

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