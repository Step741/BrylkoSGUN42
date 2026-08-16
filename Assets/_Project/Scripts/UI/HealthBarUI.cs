using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Health health;

    [SerializeField]
    private Slider healthSlider;

    private void OnEnable()
    {
        if (health != null)
        {
            health.HealthChanged += OnHealthChanged;
        }
    }

    private void Start()
    {
        if (health == null)
        {
            Debug.LogError("HealthBarUI: Health reference is missing.");
            return;
        }

        UpdateHealthUI(
            health.CurrentHealth,
            health.MaxHealth
        );
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(
        float currentHealth,
        float maxHealth)
    {
        UpdateHealthUI(
            currentHealth,
            maxHealth
        );
    }

    private void UpdateHealthUI(
        float currentHealth,
        float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
}