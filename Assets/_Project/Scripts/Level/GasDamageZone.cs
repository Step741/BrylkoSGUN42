using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasDamageZone : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private float damageInterval = 1f;

    [Header("Target")]
    [SerializeField] private LayerMask targetLayers;

    private readonly Dictionary<IDamageable, int> targetContacts =
        new Dictionary<IDamageable, int>();

    private readonly Dictionary<IDamageable, Coroutine> damageCoroutines =
        new Dictionary<IDamageable, Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTargetLayer(other.gameObject.layer))
            return;

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        if (targetContacts.ContainsKey(damageable))
        {
            targetContacts[damageable]++;
            return;
        }

        targetContacts.Add(damageable, 1);

        Coroutine coroutine =
            StartCoroutine(DamageRoutine(damageable));

        damageCoroutines.Add(damageable, coroutine);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTargetLayer(other.gameObject.layer))
            return;

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        if (!targetContacts.ContainsKey(damageable))
            return;

        targetContacts[damageable]--;

        if (targetContacts[damageable] > 0)
            return;

        StopDamageRoutine(damageable);
    }

    private IEnumerator DamageRoutine(
        IDamageable damageable)
    {
        while (true)
        {
            damageable.TakeDamage(damage);

            yield return new WaitForSeconds(
                damageInterval);
        }
    }

    private void StopDamageRoutine(
        IDamageable damageable)
    {
        if (damageCoroutines.TryGetValue(
                damageable,
                out Coroutine coroutine))
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        damageCoroutines.Remove(damageable);
        targetContacts.Remove(damageable);
    }

    private bool IsTargetLayer(int layer)
    {
        return (targetLayers.value &
                (1 << layer)) != 0;
    }

    private void OnDisable()
    {
        foreach (Coroutine coroutine in
                 damageCoroutines.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        damageCoroutines.Clear();
        targetContacts.Clear();
    }
}