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


        // Скрываем весь HUD.
        if (hud != null)
        {
            hud.SetActive(
                false
            );
        }


        // Отключаем управление персонажем.
        if (playerController != null)
        {
            playerController.enabled =
                false;
        }


        // Отключаем управление камерой.
        if (cameraController != null)
        {
            cameraController.enabled =
                false;
        }


        // Отключаем ADS.
        if (adsController != null)
        {
            adsController.enabled =
                false;
        }


        // Отключаем стрельбу, катану,
        // смену оружия и другие действия.
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


        // Запускаем Death-анимацию.
        if (animator != null)
        {
            animator.SetTrigger(
                deathTrigger
            );
        }


        // Освобождаем курсор.
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible =
            true;


        Debug.Log(
            "PLAYER DIED"
        );
    }
}