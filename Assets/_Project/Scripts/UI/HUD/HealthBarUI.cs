using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Health health;

    [SerializeField]
    private Slider healthSlider;


    [Header("DOTween Animation")]

    [SerializeField]
    [Tooltip("Длительность анимации изменения здоровья.")]
    private float healthAnimationDuration = 0.25f;

    [SerializeField]
    [Tooltip("Длительность плавной смены цвета.")]
    private float colorAnimationDuration = 0.2f;


    [Header("Colors")]

    [SerializeField]
    private Color highHealthColor = Color.green;

    [SerializeField]
    private Color mediumHealthColor = Color.yellow;

    [SerializeField]
    private Color lowHealthColor = Color.red;


    private Image fillImage;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (
            healthSlider != null &&
            healthSlider.fillRect != null
        )
        {
            fillImage =
                healthSlider
                    .fillRect
                    .GetComponent<Image>();
        }
    }


    private void OnEnable()
    {
        if (health != null)
        {
            health.HealthChanged +=
                OnHealthChanged;
        }
    }


    private void Start()
    {
        if (health == null)
        {
            Debug.LogError(
                "HealthBarUI: Health reference is missing."
            );

            return;
        }


        if (healthSlider == null)
        {
            Debug.LogError(
                "HealthBarUI: Health Slider reference is missing."
            );

            return;
        }


        // Устанавливаем максимальное здоровье.
        healthSlider.maxValue =
            health.MaxHealth;


        // Начальное значение без анимации.
        healthSlider.value =
            health.CurrentHealth;


        // Начальный цвет без анимации.
        Color startColor =
            GetHealthColor(
                health.CurrentHealth,
                health.MaxHealth
            );


        if (fillImage != null)
        {
            fillImage.color =
                startColor;
        }
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -=
                OnHealthChanged;
        }


        KillTweens();
    }


    private void OnDestroy()
    {
        // Дополнительная страховка при уничтожении
        // объекта вместе со сценой.
        KillTweens();
    }


    // =========================================================
    // HEALTH CHANGED
    // =========================================================

    private void OnHealthChanged(
        float currentHealth,
        float maxHealth)
    {
        if (healthSlider == null)
            return;


        // Обновляем максимум здоровья.
        healthSlider.maxValue =
            maxHealth;


        // Останавливаем предыдущую анимацию,
        // чтобы при быстром получении урона tween'ы
        // не накладывались друг на друга.
        healthSlider.DOKill();


        // Анимируем здоровье от текущего
        // отображаемого значения к новому.
        healthSlider
            .DOValue(
                currentHealth,
                healthAnimationDuration
            )
            .SetEase(
                Ease.OutQuad
            )
            .SetLink(
                healthSlider.gameObject
            );


        // =====================================================
        // COLOR
        // =====================================================

        if (fillImage != null)
        {
            Color targetColor =
                GetHealthColor(
                    currentHealth,
                    maxHealth
                );


            // Останавливаем предыдущую
            // анимацию цвета.
            fillImage.DOKill();


            // Плавно меняем цвет полоски.
            fillImage
                .DOColor(
                    targetColor,
                    colorAnimationDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
                .SetLink(
                    fillImage.gameObject
                );
        }
    }


    // =========================================================
    // GET HEALTH COLOR
    // =========================================================

    private Color GetHealthColor(
        float currentHealth,
        float maxHealth)
    {
        if (maxHealth <= 0f)
            return lowHealthColor;


        float healthPercent =
            currentHealth /
            maxHealth;


        if (healthPercent > 0.6f)
        {
            return highHealthColor;
        }


        if (healthPercent > 0.3f)
        {
            return mediumHealthColor;
        }


        return lowHealthColor;
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillTweens()
    {
        if (healthSlider != null)
        {
            healthSlider.DOKill();
        }


        if (fillImage != null)
        {
            fillImage.DOKill();
        }
    }
}