using UnityEngine;

public class PlayerDeathController : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Health health;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private CameraController cameraController;

    [SerializeField]
    private ADSController adsController;

    [SerializeField]
    private WeaponSwitcher weaponSwitcher;

    [Header("HUD")]

    [SerializeField]
    private GameObject hud;

    [Header("Disable On Death")]

    [SerializeField]
    private MonoBehaviour[] componentsToDisable;

    [Header("Animation")]

    [SerializeField]
    private string deathTrigger = "Death";

    private bool isDead;

    private void Awake()
    {
        if (health == null)
        {
            health =
                GetComponent<Health>();
        }


        if (characterController == null)
        {
            characterController =
                GetComponent<CharacterController>();
        }


        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }


        if (weaponSwitcher == null)
        {
            weaponSwitcher =
                GetComponent<WeaponSwitcher>();
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Died +=
                HandleDeath;
        }
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -=
                HandleDeath;
        }
    }

    private void HandleDeath()
    {
        if (isDead)
            return;


        isDead = true;


        weaponSwitcher
            ?.CancelCurrentWeaponReload();

        if (hud != null)
        {
            hud.SetActive(
                false
            );
        }

        if (playerController != null)
        {
            playerController.enabled =
                false;
        }

        if (cameraController != null)
        {
            cameraController.enabled =
                false;
        }

        if (adsController != null)
        {
            adsController.enabled =
                false;
        }

        if (componentsToDisable != null)
        {
            foreach (
                MonoBehaviour component
                in componentsToDisable
            )
            {
                if (component != null)
                {
                    component.enabled =
                        false;
                }
            }
        }

        if (animator != null)
        {
            animator.SetTrigger(
                deathTrigger
            );
        }

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible =
            true;
    }
}