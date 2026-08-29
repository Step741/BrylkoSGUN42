using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]

    [SerializeField]
    private float maxHealth = 100f;

    [Header("Heal Over Time")]

    [SerializeField]
    [Min(0.01f)]
    private float healTickInterval = 0.5f;

    [SerializeField]
    [Min(0.01f)]
    private float healPerTick = 5f;

    private float currentHealth;

    private Coroutine healCoroutine;

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


    private void OnDisable()
    {
        CancelHealing();
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
        if (IsDead)
        {
            return;
        }


        if (damage <= 0f)
        {
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

        HealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

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

    public void HealOverTime(
        float totalAmount
    )
    {
        if (IsDead)
            return;


        if (totalAmount <= 0f)
            return;


        if (currentHealth >= maxHealth)
            return;

        CancelHealing();


        healCoroutine =
            StartCoroutine(
                HealOverTimeRoutine(
                    totalAmount
                )
            );
    }


    private IEnumerator HealOverTimeRoutine(
        float totalAmount
    )
    {
        float remainingAmount =
            totalAmount;


        while (
            remainingAmount > 0f &&
            !IsDead &&
            currentHealth < maxHealth
        )
        {
            float amountThisTick =
                Mathf.Min(
                    healPerTick,
                    remainingAmount,
                    maxHealth - currentHealth
                );


            Heal(
                amountThisTick
            );


            remainingAmount -=
                amountThisTick;

            if (
                remainingAmount <= 0f ||
                currentHealth >= maxHealth ||
                IsDead
            )
            {
                break;
            }


            yield return new WaitForSeconds(
                healTickInterval
            );
        }


        healCoroutine =
            null;
    }

    public void CancelHealing()
    {
        if (healCoroutine == null)
            return;


        StopCoroutine(
            healCoroutine
        );

        healCoroutine =
            null;
    }

    private void Die()
    {
        if (IsDead)
            return;


        CancelHealing();


        IsDead =
            true;


        Died?.Invoke();
    }
}