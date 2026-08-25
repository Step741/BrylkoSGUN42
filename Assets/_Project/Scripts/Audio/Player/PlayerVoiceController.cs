using UnityEngine;
using Zenject;

public class PlayerVoiceController : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Health health;


    [Header("Jump Sounds")]

    [SerializeField]
    private AudioClip[] jumpSounds;


    [Header("Damage Sounds")]

    [SerializeField]
    private AudioClip[] damageSounds;


    [Header("Cough Sounds")]

    [SerializeField]
    private AudioClip[] coughSounds;


    [Header("Death Sounds")]

    [SerializeField]
    private AudioClip[] deathSounds;


    [Header("Volume")]

    [SerializeField]
    [Range(0f, 1f)]
    private float volume = 1f;


    [Header("Pitch Randomization")]

    [SerializeField]
    private Vector2 pitchRange =
        new Vector2(0.95f, 1.05f);


    private ISoundService soundService;


    [Inject]
    private void Construct(
        ISoundService soundService)
    {
        this.soundService = soundService;
    }


    private void Awake()
    {
        if (health == null)
        {
            health =
                GetComponent<Health>();
        }
    }


    private void OnEnable()
    {
        if (health == null)
            return;

        health.DamageReceived +=
            HandleDamageReceived;

        health.Died +=
            HandleDeath;
    }


    private void OnDisable()
    {
        if (health == null)
            return;

        health.DamageReceived -=
            HandleDamageReceived;

        health.Died -=
            HandleDeath;
    }


    // ==========================================
    // EVENTS
    // ==========================================

    private void HandleDamageReceived(
        Vector3 damageSourcePosition)
    {
        PlayDamage();
    }


    private void HandleDeath()
    {
        PlayDeath();
    }


    // ==========================================
    // PUBLIC VOICE METHODS
    // ==========================================

    public void PlayJump()
    {
        PlayRandomSound(
            jumpSounds
        );
    }


    public void PlayDamage()
    {
        PlayRandomSound(
            damageSounds
        );
    }


    public void PlayCough()
    {
        PlayRandomSound(
            coughSounds
        );
    }


    public void PlayDeath()
    {
        PlayRandomSound(
            deathSounds
        );
    }


    // ==========================================
    // PLAY SOUND
    // ==========================================

    private void PlayRandomSound(
        AudioClip[] sounds)
    {
        if (
            sounds == null ||
            sounds.Length == 0
        )
        {
            return;
        }

        if (soundService == null)
            return;


        AudioClip clip =
            sounds[
                Random.Range(
                    0,
                    sounds.Length
                )
            ];


        float pitch =
            Random.Range(
                pitchRange.x,
                pitchRange.y
            );


        soundService.Play3D(
            clip,
            transform.position,
            SoundType.SFX,
            volume,
            pitch
        );
    }
}