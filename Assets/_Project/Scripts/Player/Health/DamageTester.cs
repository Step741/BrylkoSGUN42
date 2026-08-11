using UnityEngine;
using UnityEngine.InputSystem;

public class DamageTester : MonoBehaviour
{
    [SerializeField]
    private Health health;

    [SerializeField]
    private float damage = 25f;

    private void Update()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            health.TakeDamage(damage);

            Debug.Log(
                $"PLAYER DAMAGE: {health.CurrentHealth}/{health.MaxHealth}"
            );
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            health.Heal(damage);

            Debug.Log(
                $"PLAYER HEAL: {health.CurrentHealth}/{health.MaxHealth}"
            );
        }
    }
}