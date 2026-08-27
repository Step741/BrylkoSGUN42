using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionCompleteController : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private ComputerInteractable computerInteractable;

    [SerializeField]
    private GameObject missionCompletePanel;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private RectTransform missionCompleteText;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button mainMenuButton;


    [Header("Game Control")]

    [SerializeField]
    private GameObject hudToHide;

    [SerializeField]
    private WeaponSwitcher weaponSwitcher;

    [SerializeField]
    private PlayerController playerController;


    [Header("Camera / Additional Control Scripts")]

    [Tooltip(
        "Сюда перетащи скрипт, который отвечает за вращение камеры."
    )]
    [SerializeField]
    private MonoBehaviour cameraController;

    [Tooltip(
        "Сюда можно добавить ADS, стрельбу или другие скрипты, " +
        "которые продолжают получать игровой ввод после победы."
    )]
    [SerializeField]
    private MonoBehaviour[] additionalControlScripts;


    [Header("Scene")]

    [SerializeField]
    private string mainMenuSceneName = "MainMenu";


    [Header("Animation")]

    [SerializeField]
    private float panelFadeDuration = 0.35f;

    [SerializeField]
    private float titleAnimationDuration = 0.45f;

    [SerializeField]
    private float buttonsAnimationDuration = 0.3f;

    [SerializeField]
    private float buttonsDelay = 0.1f;

    [SerializeField]
    private Ease panelEase = Ease.OutQuad;

    [SerializeField]
    private Ease titleEase = Ease.OutBack;

    [SerializeField]
    private Ease buttonsEase = Ease.OutQuad;


    private RectTransform restartButtonTransform;

    private RectTransform mainMenuButtonTransform;

    private Sequence missionCompleteSequence;

    private bool isMissionCompleteShown;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        if (playerController == null)
        {
            playerController =
                FindFirstObjectByType<PlayerController>();
        }


        if (restartButton != null)
        {
            restartButtonTransform =
                restartButton.GetComponent<RectTransform>();
        }


        if (mainMenuButton != null)
        {
            mainMenuButtonTransform =
                mainMenuButton.GetComponent<RectTransform>();
        }


        PrepareMissionCompleteScreen();


        if (restartButton != null)
        {
            restartButton.onClick.AddListener(
                RestartLevel
            );
        }


        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(
                ReturnToMainMenu
            );
        }
    }


    private void Start()
    {
        if (computerInteractable == null)
        {
            Debug.LogError(
                "[MissionComplete] Computer Interactable is not assigned.",
                this
            );

            return;
        }


        computerInteractable.Activated +=
            ShowMissionCompleteScreen;
    }


    private void OnDisable()
    {
        KillTweens();
    }


    private void OnDestroy()
    {
        if (computerInteractable != null)
        {
            computerInteractable.Activated -=
                ShowMissionCompleteScreen;
        }


        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(
                RestartLevel
            );
        }


        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                ReturnToMainMenu
            );
        }


        KillTweens();
    }


    // =========================================================
    // PREPARE SCREEN
    // =========================================================

    private void PrepareMissionCompleteScreen()
    {
        if (missionCompletePanel == null)
            return;


        if (canvasGroup == null)
        {
            canvasGroup =
                missionCompletePanel.GetComponent<CanvasGroup>();
        }


        missionCompletePanel.SetActive(
            false
        );


        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;

            canvasGroup.interactable =
                false;

            canvasGroup.blocksRaycasts =
                false;
        }
    }


    // =========================================================
    // VICTORY
    // =========================================================

    private void ShowMissionCompleteScreen()
    {
        if (isMissionCompleteShown)
            return;


        isMissionCompleteShown = true;


        Debug.Log(
            "[MissionComplete] MISSION COMPLETE!"
        );


        // -----------------------------------------------------
        // СКРЫВАЕМ HUD
        // -----------------------------------------------------

        if (hudToHide != null)
        {
            hudToHide.SetActive(
                false
            );
        }


        // -----------------------------------------------------
        // БЛОКИРУЕМ ПЕРЕКЛЮЧЕНИЕ ОРУЖИЯ
        // -----------------------------------------------------

        if (weaponSwitcher != null)
        {
            weaponSwitcher.enabled =
                false;
        }


        // -----------------------------------------------------
        // БЛОКИРУЕМ ДВИЖЕНИЕ ИГРОКА
        // -----------------------------------------------------

        if (playerController != null)
        {
            playerController.enabled =
                false;
        }


        // -----------------------------------------------------
        // БЛОКИРУЕМ КАМЕРУ
        // -----------------------------------------------------

        if (cameraController != null)
        {
            cameraController.enabled =
                false;
        }


        // -----------------------------------------------------
        // БЛОКИРУЕМ ДОПОЛНИТЕЛЬНОЕ УПРАВЛЕНИЕ
        // ADS / СТРЕЛЬБА / ДРУГИЕ СКРИПТЫ
        // -----------------------------------------------------

        if (additionalControlScripts != null)
        {
            foreach (
                MonoBehaviour controlScript
                in additionalControlScripts
            )
            {
                if (controlScript != null)
                {
                    controlScript.enabled =
                        false;
                }
            }
        }


        // -----------------------------------------------------
        // ОСВОБОЖДАЕМ КУРСОР
        // -----------------------------------------------------

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;


        // -----------------------------------------------------
        // ОСТАНАВЛИВАЕМ ИГРУ
        // -----------------------------------------------------

        Time.timeScale = 0f;


        if (missionCompletePanel == null)
            return;


        // -----------------------------------------------------
        // ПОКАЗЫВАЕМ ЭКРАН ПОБЕДЫ
        // -----------------------------------------------------

        KillTweens();


        missionCompletePanel.SetActive(
            true
        );


        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;

            canvasGroup.interactable =
                false;

            canvasGroup.blocksRaycasts =
                false;
        }


        PrepareAnimationState();


        // =====================================================
        // ANIMATION
        // =====================================================

        missionCompleteSequence =
            DOTween.Sequence()
                .SetUpdate(true)
                .SetLink(gameObject);


        // -----------------------------------------------------
        // PANEL FADE
        // -----------------------------------------------------

        if (canvasGroup != null)
        {
            missionCompleteSequence.Append(
                canvasGroup
                    .DOFade(
                        1f,
                        panelFadeDuration
                    )
                    .SetEase(
                        panelEase
                    )
            );
        }


        // -----------------------------------------------------
        // TITLE
        // -----------------------------------------------------

        if (missionCompleteText != null)
        {
            missionCompleteSequence.Join(
                missionCompleteText
                    .DOScale(
                        Vector3.one,
                        titleAnimationDuration
                    )
                    .SetEase(
                        titleEase
                    )
            );
        }


        // -----------------------------------------------------
        // DELAY
        // -----------------------------------------------------

        missionCompleteSequence.AppendInterval(
            buttonsDelay
        );


        // -----------------------------------------------------
        // RESTART BUTTON
        // -----------------------------------------------------

        if (restartButtonTransform != null)
        {
            missionCompleteSequence.Append(
                restartButtonTransform
                    .DOScale(
                        Vector3.one,
                        buttonsAnimationDuration
                    )
                    .SetEase(
                        buttonsEase
                    )
            );
        }


        // -----------------------------------------------------
        // MAIN MENU BUTTON
        // -----------------------------------------------------

        if (mainMenuButtonTransform != null)
        {
            missionCompleteSequence.Append(
                mainMenuButtonTransform
                    .DOScale(
                        Vector3.one,
                        buttonsAnimationDuration
                    )
                    .SetEase(
                        buttonsEase
                    )
            );
        }


        // -----------------------------------------------------
        // ENABLE UI
        // -----------------------------------------------------

        missionCompleteSequence.OnComplete(
            () =>
            {
                if (canvasGroup != null)
                {
                    canvasGroup.interactable =
                        true;

                    canvasGroup.blocksRaycasts =
                        true;
                }

                missionCompleteSequence = null;
            }
        );
    }


    // =========================================================
    // PREPARE ANIMATION
    // =========================================================

    private void PrepareAnimationState()
    {
        if (missionCompleteText != null)
        {
            missionCompleteText.localScale =
                Vector3.zero;
        }


        if (restartButtonTransform != null)
        {
            restartButtonTransform.localScale =
                Vector3.zero;
        }


        if (mainMenuButtonTransform != null)
        {
            mainMenuButtonTransform.localScale =
                Vector3.zero;
        }
    }


    // =========================================================
    // RESTART
    // =========================================================

    public void RestartLevel()
    {
        KillTweens();


        Time.timeScale = 1f;


        Scene currentScene =
            SceneManager.GetActiveScene();


        SceneManager.LoadScene(
            currentScene.name
        );
    }


    // =========================================================
    // MAIN MENU
    // =========================================================

    public void ReturnToMainMenu()
    {
        KillTweens();


        Time.timeScale = 1f;


        MusicTransitionManager.StartMenuTransition(
            GameMusic.Instance,
            1.5f
        );


        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillTweens()
    {
        if (missionCompleteSequence != null)
        {
            missionCompleteSequence.Kill();

            missionCompleteSequence = null;
        }


        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
        }


        if (missionCompleteText != null)
        {
            missionCompleteText.DOKill();
        }


        if (restartButtonTransform != null)
        {
            restartButtonTransform.DOKill();
        }


        if (mainMenuButtonTransform != null)
        {
            mainMenuButtonTransform.DOKill();
        }
    }
}