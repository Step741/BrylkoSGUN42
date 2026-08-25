using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]

    [SerializeField]
    private float maxHealth = 100f;


    private float currentHealth;


    public float MaxHealth => maxHealth;

    public float CurrentHealth => currentHealth;


    public bool IsDead { get; private set; }


    public event Action<float, float> HealthChanged;

    public event Action Died;

    public event Action<Vector3> DamageReceived;


    private void Awake()
    {
        currentHealth =
            maxHealth;

        IsDead =
            false;
    }


    public void TakeDamage(
        float damage
    )
    {
        TakeDamage(
            damage,
            Vector3.zero
        );
    }


    public void TakeDamage(
        float damage,
        Vector3 damageSourcePosition
    )
    {
        Debug.Log(
            $"[HEALTH] {name} получил урон: {damage}"
        );


        if (IsDead)
        {
            Debug.Log(
                $"[HEALTH] {name} уже мёртв"
            );

            return;
        }


        if (damage <= 0f)
        {
            Debug.Log(
                $"[HEALTH] {name}: урон <= 0"
            );

            return;
        }


        currentHealth -=
            damage;


        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );


        Debug.Log(
            $"[HEALTH] {name}: HP = {currentHealth}/{maxHealth}"
        );


        HealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );


        // ==========================================
        // DAMAGE RECEIVED
        //
        // Вызываем событие при любом получении урона.
        // Если источник неизвестен, передаётся Vector3.zero.
        // ==========================================

        DamageReceived?.Invoke(
            damageSourcePosition
        );


        if (currentHealth <= 0f)
        {
            Die();
        }
    }


    public void Heal(
        float amount
    )
    {
        if (IsDead)
            return;


        if (amount <= 0f)
            return;


        currentHealth +=
            amount;


        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );


        HealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );
    }


    private void Die()
    {
        if (IsDead)
            return;


        IsDead =
            true;


        Died?.Invoke();
    }
}