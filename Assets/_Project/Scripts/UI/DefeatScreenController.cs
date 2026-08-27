using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DefeatScreenController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Health health;

    [SerializeField]
    private GameObject defeatPanel;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private RectTransform gameOverText;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button mainMenuButton;


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

    private Sequence defeatSequence;

    private bool isDefeatShown;


    private void Awake()
    {
        if (health == null)
        {
            health = FindFirstObjectByType<Health>();
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


        PrepareDefeatScreen();


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


    private void OnEnable()
    {
        if (health != null)
        {
            health.Died += ShowDefeatScreen;
        }
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= ShowDefeatScreen;
        }


        KillDefeatTween();
    }


    private void OnDestroy()
    {
        KillDefeatTween();


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
    }


    private void PrepareDefeatScreen()
    {
        if (defeatPanel == null)
            return;


        if (canvasGroup == null)
        {
            canvasGroup =
                defeatPanel.GetComponent<CanvasGroup>();
        }


        defeatPanel.SetActive(
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


    private void ShowDefeatScreen()
    {
        if (isDefeatShown)
            return;


        isDefeatShown = true;


        if (defeatPanel == null)
            return;


        KillDefeatTween();


        defeatPanel.SetActive(
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


        defeatSequence =
            DOTween.Sequence()
                .SetLink(gameObject);


        if (canvasGroup != null)
        {
            defeatSequence.Append(
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


        if (gameOverText != null)
        {
            defeatSequence.Join(
                gameOverText
                    .DOScale(
                        Vector3.one,
                        titleAnimationDuration
                    )
                    .SetEase(
                        titleEase
                    )
            );
        }


        defeatSequence.AppendInterval(
            buttonsDelay
        );


        if (restartButtonTransform != null)
        {
            defeatSequence.Append(
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


        if (mainMenuButtonTransform != null)
        {
            defeatSequence.Append(
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


        defeatSequence.OnComplete(
            () =>
            {
                if (canvasGroup != null)
                {
                    canvasGroup.interactable =
                        true;

                    canvasGroup.blocksRaycasts =
                        true;
                }

                defeatSequence = null;
            }
        );
    }


    private void PrepareAnimationState()
    {
        if (gameOverText != null)
        {
            gameOverText.localScale =
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


    private void KillDefeatTween()
    {
        if (defeatSequence != null)
        {
            defeatSequence.Kill();

            defeatSequence = null;
        }
    }


    public void RestartLevel()
    {
        KillDefeatTween();


        Scene currentScene =
            SceneManager.GetActiveScene();


        SceneManager.LoadScene(
            currentScene.name
        );
    }


    public void ReturnToMainMenu()
    {
        KillDefeatTween();


        MusicTransitionManager.StartMenuTransition(
            GameMusic.Instance,
            1.5f
        );


        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }
}