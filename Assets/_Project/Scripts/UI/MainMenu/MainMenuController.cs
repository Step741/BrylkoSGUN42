using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;


    [Header("Loading")]
    [SerializeField] private GameObject loadingBarObject;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text percentText;


    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float minimumLoadingTime = 1.5f;


    [Header("Panel Animation")]
    [SerializeField] private float panelAnimationDuration = 0.4f;
    [SerializeField] private float panelSlideDistance = 1200f;


    [Header("Loading Animation")]
    [SerializeField] private float loadingFadeDuration = 0.25f;
    [SerializeField] private float loadingTextFadeDuration = 0.3f;
    [SerializeField] private float loadingProgressDuration = 0.2f;
    [SerializeField] private float loadingTextSlideDistance = 20f;
    [SerializeField] private float loadingCompletePunchStrength = 0.08f;


    private bool isLoading;
    private bool isPanelAnimating;


    private RectTransform mainMenuRect;
    private RectTransform settingsRect;

    private RectTransform loadingTextRect;
    private RectTransform percentTextRect;
    private RectTransform loadingBarRect;


    // Сохраняем позиции, которые выставлены вручную в Unity
    private Vector2 mainMenuStartPosition;
    private Vector2 settingsStartPosition;

    private Vector2 loadingTextStartPosition;
    private Vector2 percentTextStartPosition;


    private CanvasGroup loadingCanvasGroup;
    private CanvasGroup loadingTextCanvasGroup;
    private CanvasGroup percentTextCanvasGroup;


    private Tween loadingProgressTween;
    private Tween loadingTextTween;
    private Tween percentTextTween;
    private Tween loadingBarPunchTween;


    // =========================
    // UNITY
    // =========================

    private void Awake()
    {
        if (mainMenuPanel != null)
        {
            mainMenuRect =
                mainMenuPanel.GetComponent<RectTransform>();

            if (mainMenuRect != null)
            {
                mainMenuStartPosition =
                    mainMenuRect.anchoredPosition;
            }
        }


        if (settingsPanel != null)
        {
            settingsRect =
                settingsPanel.GetComponent<RectTransform>();

            if (settingsRect != null)
            {
                settingsStartPosition =
                    settingsRect.anchoredPosition;
            }
        }


        if (loadingBarObject != null)
        {
            loadingCanvasGroup =
                loadingBarObject.GetComponent<CanvasGroup>();

            if (loadingCanvasGroup == null)
            {
                loadingCanvasGroup =
                    loadingBarObject.AddComponent<CanvasGroup>();
            }
        }


        if (loadingText != null)
        {
            loadingTextRect =
                loadingText.GetComponent<RectTransform>();

            if (loadingTextRect != null)
            {
                loadingTextStartPosition =
                    loadingTextRect.anchoredPosition;
            }

            loadingTextCanvasGroup =
                loadingText.GetComponent<CanvasGroup>();

            if (loadingTextCanvasGroup == null)
            {
                loadingTextCanvasGroup =
                    loadingText.gameObject.AddComponent<CanvasGroup>();
            }
        }


        if (percentText != null)
        {
            percentTextRect =
                percentText.GetComponent<RectTransform>();

            if (percentTextRect != null)
            {
                percentTextStartPosition =
                    percentTextRect.anchoredPosition;
            }

            percentTextCanvasGroup =
                percentText.GetComponent<CanvasGroup>();

            if (percentTextCanvasGroup == null)
            {
                percentTextCanvasGroup =
                    percentText.gameObject.AddComponent<CanvasGroup>();
            }
        }


        if (loadingBar != null)
        {
            loadingBarRect =
                loadingBar.GetComponent<RectTransform>();
        }
    }


    private void Start()
    {
        // На случай запуска после паузы
        Time.timeScale = 1f;


        // Главное меню — сохраняем позицию из Unity
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);

            if (mainMenuRect != null)
            {
                mainMenuRect.anchoredPosition =
                    mainMenuStartPosition;
            }
        }


        // Настройки скрыты
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);

            if (settingsRect != null)
            {
                settingsRect.anchoredPosition =
                    settingsStartPosition;
            }
        }


        // LoadingBar скрыт
        if (loadingBarObject != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingBarObject.SetActive(false);
        }


        // Начальные значения Loading UI
        ResetLoadingVisuals();
    }


    // =========================
    // PLAY
    // =========================

    public void PlayGame()
    {
        if (isLoading || isPanelAnimating)
            return;

        ClearButtonSelection();

        StartCoroutine(LoadGame());
    }


    private IEnumerator LoadGame()
    {
        isLoading = true;


        // Останавливаем анимации панелей
        mainMenuRect?.DOKill();
        settingsRect?.DOKill();


        // Скрываем меню
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);


        // Скрываем настройки
        if (settingsPanel != null)
            settingsPanel.SetActive(false);


        // Показываем LoadingBar
        yield return StartCoroutine(ShowLoadingBar());


        // Запускаем плавный переход музыки
        MusicTransitionManager.StartGameTransition(1.5f);


        // Асинхронная загрузка сцены
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(gameSceneName);

        operation.allowSceneActivation = false;


        float timer = 0f;
        float displayedProgress = 0f;


        // Реальный прогресс загрузки Unity
        while (operation.progress < 0.9f)
        {
            timer += Time.unscaledDeltaTime;


            float targetProgress =
                Mathf.Clamp01(
                    operation.progress / 0.9f
                );


            // Плавно догоняем реальный прогресс
            displayedProgress =
                Mathf.MoveTowards(
                    displayedProgress,
                    targetProgress,
                    Time.unscaledDeltaTime * 1.5f
                );


            UpdateLoadingProgress(displayedProgress);

            yield return null;
        }


        // Минимальное время отображения загрузки
        while (timer < minimumLoadingTime)
        {
            timer += Time.unscaledDeltaTime;


            float minimumProgress =
                Mathf.Clamp01(
                    timer / minimumLoadingTime
                );


            // Не позволяем полоске двигаться назад
            displayedProgress =
                Mathf.Max(
                    displayedProgress,
                    minimumProgress
                );


            UpdateLoadingProgress(displayedProgress);

            yield return null;
        }


        // Плавно доводим до 100%
        if (loadingBar != null)
        {
            loadingProgressTween?.Kill();


            loadingProgressTween =
                DOTween.To(
                    () => loadingBar.value,
                    value =>
                    {
                        loadingBar.value = value;

                        UpdatePercentText(value);
                    },
                    1f,
                    loadingProgressDuration
                )
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);


            yield return
                loadingProgressTween.WaitForCompletion();
        }
        else
        {
            UpdatePercentText(1f);
        }


        // Небольшой акцент при завершении
        if (loadingBarRect != null)
        {
            loadingBarPunchTween?.Kill();

            loadingBarPunchTween =
                loadingBarRect
                    .DOPunchScale(
                        Vector3.one *
                        loadingCompletePunchStrength,
                        0.25f,
                        6,
                        0.5f
                    )
                    .SetUpdate(true);


            yield return
                loadingBarPunchTween.WaitForCompletion();
        }


        // Небольшая пауза на 100%
        yield return new WaitForSecondsRealtime(0.15f);


        // Переходим в игровую сцену
        operation.allowSceneActivation = true;
    }


    // =========================
    // LOADING BAR
    // =========================

    private IEnumerator ShowLoadingBar()
    {
        if (loadingBarObject == null)
            yield break;


        ResetLoadingVisuals();


        loadingBarObject.SetActive(true);


        // Основной контейнер
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
        }


        // LoadingText начинает чуть ниже
        if (loadingTextRect != null)
        {
            loadingTextRect.anchoredPosition =
                loadingTextStartPosition +
                Vector2.down *
                loadingTextSlideDistance;
        }


        // Прячем текст
        if (loadingTextCanvasGroup != null)
            loadingTextCanvasGroup.alpha = 0f;


        // Процент появляется чуть позже
        if (percentTextCanvasGroup != null)
            percentTextCanvasGroup.alpha = 0f;


        // Анимация появления всего блока
        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);


        if (loadingCanvasGroup != null)
        {
            sequence.Join(
                loadingCanvasGroup
                    .DOFade(
                        1f,
                        loadingFadeDuration
                    )
                    .SetEase(Ease.OutQuad)
            );
        }


        // Loading...
        if (
            loadingTextRect != null &&
            loadingTextCanvasGroup != null
        )
        {
            sequence.Join(
                loadingTextCanvasGroup
                    .DOFade(
                        1f,
                        loadingTextFadeDuration
                    )
                    .SetEase(Ease.OutQuad)
            );


            sequence.Join(
                loadingTextRect
                    .DOAnchorPos(
                        loadingTextStartPosition,
                        loadingTextFadeDuration
                    )
                    .SetEase(Ease.OutCubic)
            );
        }


        // Процент появляется с небольшой задержкой
        if (percentTextCanvasGroup != null)
        {
            sequence.Insert(
                0.1f,
                percentTextCanvasGroup
                    .DOFade(
                        1f,
                        loadingTextFadeDuration
                    )
                    .SetEase(Ease.OutQuad)
            );
        }


        yield return sequence.WaitForCompletion();
    }


    private void ResetLoadingVisuals()
    {
        loadingProgressTween?.Kill();
        loadingTextTween?.Kill();
        percentTextTween?.Kill();
        loadingBarPunchTween?.Kill();


        if (loadingBar != null)
            loadingBar.value = 0f;


        if (percentText != null)
            percentText.text = "0%";


        if (loadingTextRect != null)
        {
            loadingTextRect.anchoredPosition =
                loadingTextStartPosition;
        }


        if (percentTextRect != null)
        {
            percentTextRect.anchoredPosition =
                percentTextStartPosition;
        }


        if (loadingBarRect != null)
        {
            loadingBarRect.localScale =
                Vector3.one;
        }
    }


    private void UpdateLoadingProgress(
        float progress
    )
    {
        progress = Mathf.Clamp01(progress);


        if (loadingBar != null)
            loadingBar.value = progress;


        UpdatePercentText(progress);
    }


    private void UpdatePercentText(
        float progress
    )
    {
        if (percentText == null)
            return;


        int percent =
            Mathf.RoundToInt(
                Mathf.Clamp01(progress) * 100f
            );


        percentText.text =
            percent + "%";
    }


    // =========================
    // SETTINGS
    // =========================

    public void OpenSettings()
    {
        if (isLoading || isPanelAnimating)
            return;

        ClearButtonSelection();

        StartCoroutine(OpenSettingsAnimation());
    }


    private IEnumerator OpenSettingsAnimation()
    {
        if (
            mainMenuRect == null ||
            settingsRect == null
        )
            yield break;


        isPanelAnimating = true;


        mainMenuRect.DOKill();
        settingsRect.DOKill();


        // Включаем Settings
        settingsPanel.SetActive(true);


        // Settings начинает СЛЕВА
        // относительно своей позиции в Unity
        settingsRect.anchoredPosition =
            settingsStartPosition +
            Vector2.left * panelSlideDistance;


        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);


        // Главное меню уезжает ВПРАВО
        sequence.Join(
            mainMenuRect
                .DOAnchorPos(
                    mainMenuStartPosition +
                    Vector2.right * panelSlideDistance,
                    panelAnimationDuration
                )
                .SetEase(Ease.InOutQuad)
        );


        // Settings въезжает СЛЕВА
        sequence.Join(
            settingsRect
                .DOAnchorPos(
                    settingsStartPosition,
                    panelAnimationDuration
                )
                .SetEase(Ease.OutCubic)
        );


        yield return sequence.WaitForCompletion();


        // Скрываем главное меню
        mainMenuPanel.SetActive(false);


        // Оставляем его справа,
        // чтобы потом вернуть обратной анимацией
        mainMenuRect.anchoredPosition =
            mainMenuStartPosition +
            Vector2.right * panelSlideDistance;


        isPanelAnimating = false;
    }


    public void CloseSettings()
    {
        if (isLoading || isPanelAnimating)
            return;

        ClearButtonSelection();

        StartCoroutine(CloseSettingsAnimation());
    }


    private IEnumerator CloseSettingsAnimation()
    {
        if (
            mainMenuRect == null ||
            settingsRect == null
        )
            yield break;


        isPanelAnimating = true;


        mainMenuRect.DOKill();
        settingsRect.DOKill();


        // Включаем главное меню
        mainMenuPanel.SetActive(true);


        // Главное меню начинает справа
        // относительно своей исходной позиции
        mainMenuRect.anchoredPosition =
            mainMenuStartPosition +
            Vector2.right * panelSlideDistance;


        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);


        // Settings уезжают ВПРАВО
        sequence.Join(
            settingsRect
                .DOAnchorPos(
                    settingsStartPosition +
                    Vector2.right * panelSlideDistance,
                    panelAnimationDuration
                )
                .SetEase(Ease.InOutQuad)
        );


        // Главное меню возвращается справа
        // в свою исходную позицию
        sequence.Join(
            mainMenuRect
                .DOAnchorPos(
                    mainMenuStartPosition,
                    panelAnimationDuration
                )
                .SetEase(Ease.OutCubic)
        );


        yield return sequence.WaitForCompletion();


        // Скрываем Settings
        settingsPanel.SetActive(false);


        // Возвращаем исходную позицию
        settingsRect.anchoredPosition =
            settingsStartPosition;


        isPanelAnimating = false;
    }


    // =========================
    // BUTTON SELECTION
    // =========================

    private void ClearButtonSelection()
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current
            .SetSelectedGameObject(null);
    }


    // =========================
    // EXIT
    // =========================

    public void ExitGame()
    {
        if (isLoading || isPanelAnimating)
            return;

        ClearButtonSelection();


#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    // =========================
    // CLEANUP
    // =========================

    private void OnDestroy()
    {
        mainMenuRect?.DOKill();
        settingsRect?.DOKill();

        loadingBarRect?.DOKill();

        loadingProgressTween?.Kill();
        loadingTextTween?.Kill();
        percentTextTween?.Kill();
        loadingBarPunchTween?.Kill();

        if (loadingCanvasGroup != null)
            loadingCanvasGroup.DOKill();

        if (loadingTextCanvasGroup != null)
            loadingTextCanvasGroup.DOKill();

        if (percentTextCanvasGroup != null)
            percentTextCanvasGroup.DOKill();
    }
}