using UnityEngine;
using Zenject;

public class FootstepController : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private PlayerController playerController;


    [Header("Surface Detection")]

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float raycastDistance = 2f;

    [SerializeField]
    private float raycastStartHeight = 0.3f;


    [Header("Footstep Timing")]

    [SerializeField]
    private float walkStepInterval = 0.5f;

    [SerializeField]
    private float sprintStepInterval = 0.35f;

    [SerializeField]
    private float crouchStepInterval = 0.7f;


    [Header("Landing")]

    [SerializeField]
    private float landingMinFallDistance = 0.5f;


    [Header("Volume")]

    [SerializeField]
    [Range(0f, 1f)]
    private float volume = 1f;


    [Header("Pitch Randomization")]

    [SerializeField]
    private Vector2 pitchRange =
        new Vector2(0.95f, 1.05f);


    [Header("Concrete")]

    [SerializeField]
    private AudioClip[] concreteSounds;


    [Header("Metal")]

    [SerializeField]
    private AudioClip[] metalSounds;


    [Header("Dirt")]

    [SerializeField]
    private AudioClip[] dirtSounds;


    private ISoundService soundService;

    private CharacterController characterController;

    private float stepTimer;

    private bool wasGrounded;

    private float airStartY;


    [Inject]
    private void Construct(
        ISoundService soundService)
    {
        this.soundService = soundService;
    }


    private void Awake()
    {
        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }

        characterController =
            GetComponent<CharacterController>();

        if (playerController != null)
        {
            wasGrounded =
                playerController.IsGrounded;
        }
    }


    private void Update()
    {
        if (playerController == null)
            return;


        bool isGrounded =
            playerController.IsGrounded;


        // ==========================================
        // AIR / LANDING DETECTION
        // ==========================================

        // Игрок только оторвался от земли
        if (!isGrounded && wasGrounded)
        {
            airStartY =
                transform.position.y;
        }


        // Игрок приземлился
        if (isGrounded && !wasGrounded)
        {
            float fallDistance =
                airStartY -
                transform.position.y;


            if (fallDistance >= landingMinFallDistance)
            {
                PlayFootstep();
            }
        }


        // ==========================================
        // NORMAL FOOTSTEPS
        // ==========================================

        if (!isGrounded)
        {
            stepTimer = 0f;

            wasGrounded = false;

            return;
        }


        if (!playerController.IsMoving)
        {
            stepTimer = 0f;

            wasGrounded = true;

            return;
        }


        stepTimer += Time.deltaTime;


        float stepInterval =
            GetCurrentStepInterval();


        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f;

            PlayFootstep();
        }


        wasGrounded = true;
    }


    // ==========================================
    // STEP INTERVAL
    // ==========================================

    private float GetCurrentStepInterval()
    {
        if (playerController.IsCrouching)
        {
            return crouchStepInterval;
        }

        if (playerController.IsSprinting)
        {
            return sprintStepInterval;
        }

        return walkStepInterval;
    }


    // ==========================================
    // PLAY FOOTSTEP
    // ==========================================

    private void PlayFootstep()
    {
        AudioClip[] sounds =
            GetSurfaceSounds();

        if (
            sounds == null ||
            sounds.Length == 0
        )
        {
            return;
        }

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

        if (soundService == null)
        {
            return;
        }

        soundService.Play3D(
            clip,
            transform.position,
            SoundType.SFX,
            volume,
            pitch
        );
    }


    // ==========================================
    // SURFACE SOUNDS
    // ==========================================

    private AudioClip[] GetSurfaceSounds()
    {
        SurfaceType surfaceType =
            GetSurfaceType();

        switch (surfaceType)
        {
            case SurfaceType.Metal:
                return metalSounds;

            case SurfaceType.Dirt:
                return dirtSounds;

            case SurfaceType.Concrete:
            default:
                return concreteSounds;
        }
    }


    // ==========================================
    // SURFACE DETECTION
    // ==========================================

    private SurfaceType GetSurfaceType()
    {
        Vector3 origin =
            GetRaycastOrigin();

        RaycastHit[] hits =
            Physics.RaycastAll(
                origin,
                Vector3.down,
                raycastDistance,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );

        if (hits.Length == 0)
        {
            return SurfaceType.Concrete;
        }

        System.Array.Sort(
            hits,
            (a, b) =>
                a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit hit in hits)
        {
            // Игнорируем коллайдеры самого Player
            if (
                hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform)
            )
            {
                continue;
            }

            SurfaceIdentifier surfaceIdentifier =
                hit.collider.GetComponent<SurfaceIdentifier>();

            // Если скрипт висит на родительском объекте
            if (surfaceIdentifier == null)
            {
                surfaceIdentifier =
                    hit.collider.GetComponentInParent<SurfaceIdentifier>();
            }

            if (surfaceIdentifier != null)
            {
                return surfaceIdentifier.SurfaceType;
            }
        }

        return SurfaceType.Concrete;
    }


    // ==========================================
    // RAYCAST ORIGIN
    // ==========================================

    private Vector3 GetRaycastOrigin()
    {
        if (characterController != null)
        {
            Bounds bounds =
                characterController.bounds;

            float footY =
                bounds.min.y;

            return new Vector3(
                bounds.center.x,
                footY + raycastStartHeight,
                bounds.center.z
            );
        }

        return
            transform.position +
            Vector3.up * raycastStartHeight;
    }
}