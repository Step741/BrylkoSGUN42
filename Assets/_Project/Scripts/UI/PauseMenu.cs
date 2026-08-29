using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
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

    [Header("Title")]

    [SerializeField]
    private RectTransform pausedTitle;

    [Header("Menu Buttons")]

    [SerializeField]
    private Button continueButton;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button settingsButton;

    [FormerlySerializedAs("quitButton")]
    [SerializeField]
    private Button mainMenuButton;

    [Header("Settings")]

    [SerializeField]
    private GameObject settingsRoot;

    [SerializeField]
    private CanvasGroup settingsCanvasGroup;

    [SerializeField]
    private RectTransform settingsPanel;

    [SerializeField]
    private Button backButton;

    [Header("Scene")]

    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    [Header("Animation - Menu")]

    [SerializeField]
    private float fadeDuration = 0.2f;

    [SerializeField]
    private float panelDuration = 0.3f;

    [SerializeField]
    private float titleDuration = 0.4f;

    [SerializeField]
    private float buttonDuration = 0.22f;

    [SerializeField]
    private float buttonDelay = 0.07f;

    [SerializeField]
    private float hiddenScale = 0.92f;

    [SerializeField]
    private float titleStartScale = 0.75f;

    [SerializeField]
    private float buttonStartScale = 0.85f;


    [Header("Animation - Settings")]

    [SerializeField]
    private float settingsFadeDuration = 0.2f;

    [SerializeField]
    private float settingsScaleDuration = 0.3f;

    private IInputService inputService;

    private bool isOpen;
    private bool isSettingsOpen;

    private bool isSceneChanging;

    private Sequence menuSequence;
    private Sequence settingsSequence;

    private Tween fadeTween;
    private Tween scaleTween;

    [Inject]
    private void Construct(
        IInputService inputService)
    {
        this.inputService = inputService;

        if (this.inputService != null)
        {
            this.inputService.Pause.performed +=
                OnPausePerformed;
        }
    }

    private void Awake()
    {
        if (menuRoot == null)
        {
            return;
        }


        if (menuCanvasGroup == null)
        {
            menuCanvasGroup =
                menuRoot.GetComponent<CanvasGroup>();
        }


        if (menuCanvasGroup == null)
        {
            return;
        }

        if (settingsRoot != null &&
            settingsCanvasGroup == null)
        {
            settingsCanvasGroup =
                settingsRoot.GetComponent<CanvasGroup>();
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(
                CloseMenu
            );
        }


        if (restartButton != null)
        {
            restartButton.onClick.AddListener(
                RestartGame
            );
        }


        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(
                OpenSettings
            );
        }


        if (backButton != null)
        {
            backButton.onClick.AddListener(
                CloseSettings
            );
        }


        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(
                ReturnToMainMenu
            );
        }


        HideImmediately();

        SetCursorForGameplay();
    }

    private void OnPausePerformed(
        InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (isSceneChanging)
            return;

        HandlePause();
    }


    private void HandlePause()
    {
        if (isSceneChanging)
            return;

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

    public void ToggleMenu()
    {
        if (isSceneChanging)
            return;


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
        if (isSceneChanging)
            return;

        if (isOpen)
            return;


        isOpen = true;
        isSettingsOpen = false;

        if (inputService != null)
        {
            inputService.EnableUIInput();
        }

        Time.timeScale = 0f;

        SetCursorForMenu();


        if (settingsRoot != null)
        {
            settingsRoot.SetActive(false);
        }

        menuRoot.SetActive(true);

        menuCanvasGroup.alpha = 0f;

        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;


        KillAllTweens();

        PrepareMenuAnimation();

        menuSequence =
            DOTween.Sequence()
                .SetUpdate(true)
                .SetLink(gameObject);

        menuSequence.Append(
            menuCanvasGroup
                .DOFade(
                    1f,
                    fadeDuration
                )
                .SetEase(Ease.OutQuad)
        );

        if (menuPanel != null)
        {
            menuSequence.Join(
                menuPanel
                    .DOScale(
                        Vector3.one,
                        panelDuration
                    )
                    .SetEase(Ease.OutQuad)
            );
        }

        if (pausedTitle != null)
        {
            menuSequence.Join(
                pausedTitle
                    .DOScale(
                        Vector3.one,
                        titleDuration
                    )
                    .SetEase(Ease.OutBack)
            );
        }

        AppendButtonAnimation(
            menuSequence,
            continueButton
        );

        AppendButtonAnimation(
            menuSequence,
            restartButton
        );

        AppendButtonAnimation(
            menuSequence,
            settingsButton
        );

        AppendButtonAnimation(
            menuSequence,
            mainMenuButton
        );

        menuSequence.OnComplete(
            () =>
            {
                if (isSceneChanging)
                    return;

                menuCanvasGroup.interactable = true;

                menuCanvasGroup.blocksRaycasts = true;

                SelectButton(
                    continueButton
                );
            }
        );
    }

    public void CloseMenu()
    {
        if (isSceneChanging)
            return;

        if (!isOpen)
            return;


        isOpen = false;
        isSettingsOpen = false;


        KillAllTweens();


        if (settingsRoot != null)
        {
            settingsRoot.SetActive(false);
        }


        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;

        fadeTween =
            menuCanvasGroup
                .DOFade(
                    0f,
                    fadeDuration
                )
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .SetLink(gameObject);

        if (menuPanel != null)
        {
            scaleTween =
                menuPanel
                    .DOScale(
                        Vector3.one * hiddenScale,
                        panelDuration
                    )
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true)
                    .SetLink(gameObject);
        }


        fadeTween.OnComplete(
            () =>
            {
                if (isSceneChanging)
                    return;

                menuRoot.SetActive(false);


                //Возвращает время
                Time.timeScale = 1f;


                //Возвращает input
                if (inputService != null)
                {
                    inputService.EnablePlayerInput();
                }


                SetCursorForGameplay();

                ClearUISelection();
            }
        );
    }

    private void PrepareMenuAnimation()
    {
        if (menuPanel != null)
        {
            menuPanel.localScale =
                Vector3.one * hiddenScale;
        }


        if (pausedTitle != null)
        {
            pausedTitle.localScale =
                Vector3.one * titleStartScale;
        }


        PrepareButton(
            continueButton
        );

        PrepareButton(
            restartButton
        );

        PrepareButton(
            settingsButton
        );

        PrepareButton(
            mainMenuButton
        );
    }


    private void PrepareButton(
        Button button)
    {
        if (button == null)
            return;


        RectTransform rectTransform =
            button.GetComponent<RectTransform>();


        if (rectTransform != null)
        {
            rectTransform.localScale =
                Vector3.one * buttonStartScale;
        }


        CanvasGroup canvasGroup =
            GetOrAddCanvasGroup(
                button.gameObject
            );


        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }


    private void AppendButtonAnimation(
        Sequence sequence,
        Button button)
    {
        if (button == null)
            return;


        RectTransform rectTransform =
            button.GetComponent<RectTransform>();


        CanvasGroup canvasGroup =
            GetOrAddCanvasGroup(
                button.gameObject
            );


        if (rectTransform == null)
            return;


        sequence.AppendInterval(
            buttonDelay
        );


        sequence.Append(
            rectTransform
                .DOScale(
                    Vector3.one,
                    buttonDuration
                )
                .SetEase(Ease.OutBack)
        );


        if (canvasGroup != null)
        {
            sequence.Join(
                canvasGroup
                    .DOFade(
                        1f,
                        buttonDuration
                    )
            );
        }
    }

    public void OpenSettings()
    {
        if (isSceneChanging)
            return;

        if (!isOpen)
            return;


        if (settingsRoot == null)
            return;


        if (isSettingsOpen)
            return;


        isSettingsOpen = true;


        KillSettingsTween();


        //Основное меню временно блокирует
        menuCanvasGroup.interactable = false;

        menuCanvasGroup.blocksRaycasts = false;


        settingsRoot.SetActive(true);


        if (settingsCanvasGroup != null)
        {
            settingsCanvasGroup.alpha = 0f;

            settingsCanvasGroup.interactable = false;

            settingsCanvasGroup.blocksRaycasts = false;
        }


        if (settingsPanel != null)
        {
            settingsPanel.localScale =
                Vector3.one * hiddenScale;
        }

        settingsSequence =
            DOTween.Sequence()
                .SetUpdate(true)
                .SetLink(gameObject);

        settingsSequence.Append(
            menuCanvasGroup
                .DOFade(
                    0f,
                    settingsFadeDuration
                )
                .SetEase(Ease.InQuad)
        );


        settingsSequence.AppendCallback(
            () =>
            {
                if (isSceneChanging)
                    return;

                if (menuPanel != null)
                {
                    menuPanel.gameObject.SetActive(
                        false
                    );
                }
            }
        );

        if (settingsCanvasGroup != null)
        {
            settingsSequence.Append(
                settingsCanvasGroup
                    .DOFade(
                        1f,
                        settingsFadeDuration
                    )
                    .SetEase(Ease.OutQuad)
            );
        }


        if (settingsPanel != null)
        {
            settingsSequence.Join(
                settingsPanel
                    .DOScale(
                        Vector3.one,
                        settingsScaleDuration
                    )
                    .SetEase(Ease.OutBack)
            );
        }


        settingsSequence.OnComplete(
            () =>
            {
                if (isSceneChanging)
                    return;

                if (settingsCanvasGroup != null)
                {
                    settingsCanvasGroup.interactable =
                        true;

                    settingsCanvasGroup.blocksRaycasts =
                        true;
                }


                SelectButton(
                    backButton
                );
            }
        );
    }

    public void CloseSettings()
    {
        if (isSceneChanging)
            return;

        if (!isSettingsOpen)
            return;


        isSettingsOpen = false;


        KillSettingsTween();


        if (settingsCanvasGroup != null)
        {
            settingsCanvasGroup.interactable = false;

            settingsCanvasGroup.blocksRaycasts = false;
        }


        settingsSequence =
            DOTween.Sequence()
                .SetUpdate(true)
                .SetLink(gameObject);

        if (settingsCanvasGroup != null)
        {
            settingsSequence.Append(
                settingsCanvasGroup
                    .DOFade(
                        0f,
                        settingsFadeDuration
                    )
                    .SetEase(Ease.InQuad)
            );
        }
        else
        {
            settingsSequence.AppendInterval(
                settingsFadeDuration
            );
        }


        settingsSequence.AppendCallback(
            () =>
            {
                if (isSceneChanging)
                    return;

                settingsRoot.SetActive(false);


                if (menuPanel != null)
                {
                    menuPanel.gameObject.SetActive(
                        true
                    );

                    menuPanel.localScale =
                        Vector3.one * hiddenScale;
                }


                menuCanvasGroup.alpha = 0f;
            }
        );

        settingsSequence.Append(
            menuCanvasGroup
                .DOFade(
                    1f,
                    settingsFadeDuration
                )
                .SetEase(Ease.OutQuad)
        );


        if (menuPanel != null)
        {
            settingsSequence.Join(
                menuPanel
                    .DOScale(
                        Vector3.one,
                        settingsScaleDuration
                    )
                    .SetEase(Ease.OutBack)
            );
        }


        settingsSequence.OnComplete(
            () =>
            {
                if (isSceneChanging)
                    return;

                menuCanvasGroup.interactable = true;

                menuCanvasGroup.blocksRaycasts = true;


                SelectButton(
                    continueButton
                );
            }
        );
    }

    public void RestartGame()
    {
        if (isSceneChanging)
            return;


        isSceneChanging = true;


        PrepareForSceneChange();


        StartCoroutine(
            RestartSceneNextFrame()
        );
    }


    private IEnumerator RestartSceneNextFrame()
    {
        yield return null;


        Scene currentScene =
            SceneManager.GetActiveScene();


        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    public void ReturnToMainMenu()
    {
        if (isSceneChanging)
            return;


        isSceneChanging = true;


        PrepareForSceneChange();


        MusicTransitionManager.StartMenuTransition(
            GameMusic.Instance,
            1.5f
        );


        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    private void PrepareForSceneChange()
    {
        Time.timeScale = 1f;


        isOpen = false;
        isSettingsOpen = false;


        if (inputService != null)
        {
            inputService.EnablePlayerInput();
        }


        ClearUISelection();

        KillAllTweens();


        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible =
            true;
    }

    private void SelectButton(
        Button button)
    {
        if (button == null)
            return;


        EventSystem eventSystem =
            EventSystem.current;


        if (eventSystem == null)
            return;


        eventSystem.SetSelectedGameObject(
            null
        );


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


        eventSystem.SetSelectedGameObject(
            null
        );
    }

    private void SetCursorForMenu()
    {
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible =
            true;
    }


    private void SetCursorForGameplay()
    {
        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible =
            false;
    }

    private CanvasGroup GetOrAddCanvasGroup(
        GameObject target)
    {
        if (target == null)
            return null;


        CanvasGroup canvasGroup =
            target.GetComponent<CanvasGroup>();


        if (canvasGroup == null)
        {
            canvasGroup =
                target.AddComponent<CanvasGroup>();
        }


        return canvasGroup;
    }

    private void KillMenuTweens()
    {
        fadeTween?.Kill();

        scaleTween?.Kill();

        fadeTween = null;

        scaleTween = null;
    }


    private void KillSettingsTween()
    {
        settingsSequence?.Kill();

        settingsSequence = null;
    }


    private void KillAllTweens()
    {
        KillMenuTweens();

        menuSequence?.Kill();

        menuSequence = null;

        KillSettingsTween();
    }

    private void HideImmediately()
    {
        KillAllTweens();


        isOpen = false;

        isSettingsOpen = false;

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;

            menuCanvasGroup.interactable =
                false;

            menuCanvasGroup.blocksRaycasts =
                false;
        }


        if (menuPanel != null)
        {
            menuPanel.localScale =
                Vector3.one * hiddenScale;
        }


        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }

        if (settingsCanvasGroup != null)
        {
            settingsCanvasGroup.alpha = 0f;

            settingsCanvasGroup.interactable =
                false;

            settingsCanvasGroup.blocksRaycasts =
                false;
        }


        if (settingsRoot != null)
        {
            settingsRoot.SetActive(false);
        }
    }

    private void OnApplicationFocus(
        bool hasFocus)
    {
        if (!hasFocus)
            return;

        if (isSceneChanging)
            return;


        if (isOpen)
        {
            SetCursorForMenu();


            if (isSettingsOpen)
            {
                SelectButton(
                    backButton
                );
            }
            else
            {
                SelectButton(
                    continueButton
                );
            }
        }
        else
        {
            SetCursorForGameplay();
        }
    }

    private void OnDestroy()
    {
        KillAllTweens();


        if (inputService != null)
        {
            inputService.Pause.performed -=
                OnPausePerformed;
        }


        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                CloseMenu
            );
        }


        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(
                RestartGame
            );
        }


        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(
                OpenSettings
            );
        }


        if (backButton != null)
        {
            backButton.onClick.RemoveListener(
                CloseSettings
            );
        }


        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                ReturnToMainMenu
            );
        }


        Time.timeScale = 1f;
    }
}