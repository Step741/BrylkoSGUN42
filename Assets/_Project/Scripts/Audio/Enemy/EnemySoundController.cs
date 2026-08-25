using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class EnemySoundController : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Health health;

    [SerializeField]
    private NavMeshAgent agent;


    [Header("Footsteps")]

    [SerializeField]
    private AudioClip[] footstepSounds;

    [SerializeField]
    private float footstepInterval = 0.5f;

    [SerializeField]
    private float minimumMoveSpeed = 0.1f;


    [Header("Idle Sounds")]

    [SerializeField]
    private AudioClip[] idleSounds;

    [SerializeField]
    private float minIdleInterval = 6f;

    [SerializeField]
    private float maxIdleInterval = 12f;


    [Header("Alert Sounds")]

    [SerializeField]
    private AudioClip[] alertSounds;


    [Header("Attack Sounds")]

    [SerializeField]
    private AudioClip[] attackSounds;


    [Header("Claw Hit Sounds")]

    [SerializeField]
    private AudioClip[] clawHitSounds;


    [Header("Hurt Sounds")]

    [SerializeField]
    private AudioClip[] hurtSounds;


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


    [Header("3D Sound Settings")]

    [SerializeField]
    private float minDistance = 3f;

    [SerializeField]
    private float maxDistance = 25f;


    private ISoundService soundService;

    private float footstepTimer;

    private float idleTimer;
    private float nextIdleTime;

    private bool isDead;


    // ==========================================
    // ZENJECT
    // ==========================================

    [Inject]
    private void Construct(
        ISoundService soundService)
    {
        this.soundService = soundService;
    }


    // ==========================================
    // UNITY
    // ==========================================

    private void Awake()
    {
        if (health == null)
        {
            health =
                GetComponent<Health>();
        }

        if (agent == null)
        {
            agent =
                GetComponent<NavMeshAgent>();
        }

        ResetIdleTimer();
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


    private void Update()
    {
        if (isDead)
            return;

        UpdateFootsteps();
        UpdateIdleSounds();
    }


    // ==========================================
    // FOOTSTEPS
    // ==========================================

    private void UpdateFootsteps()
    {
        if (
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh
        )
        {
            footstepTimer = 0f;

            return;
        }


        float currentSpeed =
            agent.velocity.magnitude;


        if (currentSpeed < minimumMoveSpeed)
        {
            footstepTimer = 0f;

            return;
        }


        footstepTimer +=
            Time.deltaTime;


        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;

            PlayRandomSound(
                footstepSounds
            );
        }
    }


    // ==========================================
    // IDLE SOUNDS
    // ==========================================

    private void UpdateIdleSounds()
    {
        if (
            idleSounds == null ||
            idleSounds.Length == 0
        )
        {
            return;
        }


        idleTimer +=
            Time.deltaTime;


        if (idleTimer >= nextIdleTime)
        {
            PlayRandomSound(
                idleSounds
            );

            ResetIdleTimer();
        }
    }


    private void ResetIdleTimer()
    {
        idleTimer = 0f;

        nextIdleTime =
            Random.Range(
                minIdleInterval,
                maxIdleInterval
            );
    }


    // ==========================================
    // HEALTH EVENTS
    // ==========================================

    private void HandleDamageReceived(
        Vector3 damageSourcePosition)
    {
        if (isDead)
            return;

        PlayHurt();
    }


    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;

        footstepTimer = 0f;
        idleTimer = 0f;

        PlayDeath();
    }


    // ==========================================
    // PUBLIC SOUND METHODS
    // ==========================================

    public void PlayAlert()
    {
        if (isDead)
            return;

        PlayRandomSound(
            alertSounds
        );
    }


    public void PlayAttack()
    {
        if (isDead)
            return;

        PlayRandomSound(
            attackSounds
        );
    }


    public void PlayClawHit()
    {
        if (isDead)
            return;

        PlayRandomSound(
            clawHitSounds
        );
    }


    public void PlayHurt()
    {
        if (isDead)
            return;

        PlayRandomSound(
            hurtSounds
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
            pitch,
            minDistance,
            maxDistance
        );
    }
}