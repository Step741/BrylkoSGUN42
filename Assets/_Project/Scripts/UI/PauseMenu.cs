using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Zenject;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField]
    private GameObject menuRoot;

    [SerializeField]
    private CanvasGroup menuCanvasGroup;

    [SerializeField]
    private RectTransform menuPanel;

    [Header("Menu Buttons")]
    [SerializeField]
    private Button continueButton;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button settingsButton;

    [SerializeField]
    private Button quitButton;

    [Header("Settings")]
    [SerializeField]
    private GameObject settingsRoot;

    [SerializeField]
    private Slider volumeSlider;

    [SerializeField]
    private TMP_Text volumeText;

    [SerializeField]
    private Button backButton;

    [Header("Animation")]
    [SerializeField]
    private float fadeDuration = 0.2f;

    [SerializeField]
    private float scaleDuration = 0.25f;

    [SerializeField]
    private float hiddenScale = 0.92f;


    private IInputService inputService;

    private bool isOpen;
    private bool isSettingsOpen;

    private Tween fadeTween;
    private Tween scaleTween;


    // =========================================================
    // ZENJECT
    // =========================================================

    [Inject]
    private void Construct(IInputService inputService)
    {
        this.inputService = inputService;

        if (this.inputService != null)
        {
            this.inputService.Pause.performed += OnPausePerformed;
        }
    }


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        if (menuRoot == null)
        {
            Debug.LogError(
                "PauseMenu: Menu Root reference is missing.",
                this
            );

            return;
        }

        if (menuCanvasGroup == null)
        {
            menuCanvasGroup =
                menuRoot.GetComponent<CanvasGroup>();
        }

        if (menuCanvasGroup == null)
        {
            Debug.LogError(
                "PauseMenu: CanvasGroup is missing on MenuRoot.",
                this
            );

            return;
        }

        LoadVolume();

        HideImmediately();

        if (settingsRoot != null)
        {
            settingsRoot.SetActive(false);
        }

        SetCursorForGameplay();
    }


    // =========================================================
    // PAUSE INPUT
    // =========================================================

    private void OnPausePerformed(
        InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        HandlePause();
    }


    private void HandlePause()
    {
        // Если открыты настройки —
        // Esc сначала возвращает в главное меню.
        if (isSettingsOpen)
        {
            CloseSettings();
            return;
        }

        if (isOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }


    // =========================================================
    // MENU
    // =========================================================

    public void ToggleMenu()
    {
        if (isOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }


    public void OpenMenu()
    {
        if (isOpen)
            return;

        isOpen = true;
        isSettingsOpen = false;


        // -----------------------------------------------------
        // INPUT
        // -----------------------------------------------------

        if (inputService != null)
        {
            inputService.EnableUIInput();
        }


        // -----------------------------------------------------
        // PAUSE
        // -----------------------------------------------------

        Time.timeScale = 0f;


        // -----------------------------------------------------
        // CURSOR
        // -----------------------------------------------------

        SetCursorForMenu();


        // -----------------------------------------------------
        // SETTINGS
        // -----------------------------------------------------

        if (settingsRoot != null)
        {
            settingsRoot.SetActive(false);
        }


        // -----------------------------------------------------
        // MENU
        // -----------------------------------------------------

        menuRoot.SetActive(true);

        menuCanvasGroup.interactable = true;
        menuCanvasGroup.blocksRaycasts = true;


        KillMenuTweens();


        menuCanvasGroup.alpha = 0f;


        if (menuPanel != null)
        {
            menuPanel.localScale =
                Vector3.one * hiddenScale;
        }


        // -----------------------------------------------------
        // UI FOCUS
        // -----------------------------------------------------

        SelectButton(continueButton);


        // -----------------------------------------------------
        // FADE
        // -----------------------------------------------------

        fadeTween =
            menuCanvasGroup
                .DOFade(
                    1f,
                    fadeDuration
                )
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetLink(gameObject);


        // -----------------------------------------------------
        // SCALE
        // -----------------------------------------------------

        if (menuPanel != null)
        {
            scaleTween =
                menuPanel
                    .DOScale(
                        Vector3.one,
                        scaleDuration
                    )
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .SetLink(gameObject);
        }
    }


    public void CloseMenu()
    {
        if (!isOpen)
            return;

        isOpen = false;
        isSettingsOpen = false;


        if (settingsRoot != null)
        {
            settingsRoot.SetActive(false);
        }


        KillMenuTweens();


        // -----------------------------------------------------
        // UI
        // -----------------------------------------------------

        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;


        // -----------------------------------------------------
        // FADE OUT
        // -----------------------------------------------------

        fadeTween =
            menuCanvasGroup
                .DOFade(
                    0f,
                    fadeDuration
                )
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .SetLink(gameObject);


        // -----------------------------------------------------
        // SCALE DOWN
        // -----------------------------------------------------

        if (menuPanel != null)
        {
            scaleTween =
                menuPanel
                    .DOScale(
                        Vector3.one * hiddenScale,
                        scaleDuration
                    )
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true)
                    .SetLink(gameObject);
        }


        fadeTween.OnComplete(() =>
        {
            menuRoot.SetActive(false);


            // Возвращаем время.
            Time.timeScale = 1f;


            // Возвращаем управление игроку.
            if (inputService != null)
            {
                inputService.EnablePlayerInput();
            }


            // Возвращаем игровой курсор.
            SetCursorForGameplay();


            // Убираем UI selection.
            ClearUISelection();
        });
    }


    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (!isOpen)
            return;

        if (settingsRoot == null)
            return;

        isSettingsOpen = true;

        // Скрываем основную панель меню.
        if (menuPanel != null)
            menuPanel.gameObject.SetActive(false);

        // Показываем настройки.
        settingsRoot.SetActive(true);

        // Фокусируем кнопку BACK.
        SelectButton(backButton);
    }


    public void CloseSettings()
    {
        if (settingsRoot == null)
            return;

        isSettingsOpen = false;

        // Скрываем настройки.
        settingsRoot.SetActive(false);

        // Возвращаем основное меню.
        if (menuPanel != null)
            menuPanel.gameObject.SetActive(true);

        // Возвращаем фокус на CONTINUE.
        SelectButton(continueButton);
    }


    // =========================================================
    // VOLUME
    // =========================================================

    private void LoadVolume()
    {
        float volume =
            PlayerPrefs.GetFloat(
                "MasterVolume",
                1f
            );


        AudioListener.volume = volume;


        if (volumeSlider != null)
        {
            volumeSlider.value = volume;

            volumeSlider.onValueChanged.AddListener(
                SetVolume
            );
        }


        UpdateVolumeText(volume);
    }


    public void SetVolume(float value)
    {
        value =
            Mathf.Clamp01(value);


        AudioListener.volume = value;


        PlayerPrefs.SetFloat(
            "MasterVolume",
            value
        );


        PlayerPrefs.Save();


        UpdateVolumeText(value);
    }


    private void UpdateVolumeText(float value)
    {
        if (volumeText == null)
            return;


        int percent =
            Mathf.RoundToInt(
                value * 100f
            );


        volumeText.text =
            $"Громкость: {percent}%";
    }


    // =========================================================
    // RESTART
    // =========================================================

    public void RestartGame()
    {
        Time.timeScale = 1f;


        if (inputService != null)
        {
            inputService.EnablePlayerInput();
        }


        SetCursorForGameplay();


        ClearUISelection();


        KillMenuTweens();


        Scene currentScene =
            SceneManager.GetActiveScene();


        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }


    // =========================================================
    // QUIT
    // =========================================================

    public void QuitGame()
    {
        Time.timeScale = 1f;


        if (inputService != null)
        {
            inputService.EnablePlayerInput();
        }


        SetCursorForGameplay();


        ClearUISelection();


#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying =
            false;

#else

        Application.Quit();

#endif
    }


    // =========================================================
    // UI FOCUS
    // =========================================================

    private void SelectButton(Button button)
    {
        if (button == null)
            return;

        EventSystem eventSystem =
            EventSystem.current;

        if (eventSystem == null)
            return;


        eventSystem.SetSelectedGameObject(null);

        eventSystem.SetSelectedGameObject(
            button.gameObject
        );
    }


    private void ClearUISelection()
    {
        EventSystem eventSystem =
            EventSystem.current;

        if (eventSystem == null)
            return;


        eventSystem.SetSelectedGameObject(null);
    }


    // =========================================================
    // CURSOR
    // =========================================================

    private void SetCursorForMenu()
    {
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }


    private void SetCursorForGameplay()
    {
        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;
    }


    // =========================================================
    // TWEENS
    // =========================================================

    private void KillMenuTweens()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();


        fadeTween = null;
        scaleTween = null;
    }


    private void HideImmediately()
    {
        KillMenuTweens();


        isOpen = false;
        isSettingsOpen = false;


        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }


        if (menuPanel != null)
        {
            menuPanel.localScale =
                Vector3.one * hiddenScale;
        }


        menuRoot.SetActive(false);
    }


    // =========================================================
    // EDITOR / FOCUS
    // =========================================================

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            return;


        if (isOpen)
        {
            SetCursorForMenu();

            if (isSettingsOpen)
            {
                SelectButton(backButton);
            }
            else
            {
                SelectButton(continueButton);
            }
        }
        else
        {
            SetCursorForGameplay();
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        KillMenuTweens();


        if (inputService != null)
        {
            inputService.Pause.performed -=
                OnPausePerformed;
        }


        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(
                SetVolume
            );
        }


        Time.timeScale = 1f;
    }
}