using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasDamageZone : MonoBehaviour
{
    [Header("Damage")]

    [SerializeField]
    private float damage = 1f;

    [SerializeField]
    private float damageInterval = 1f;


    [Header("Target")]

    [SerializeField]
    private LayerMask targetLayers;


    [Header("Gas")]

    [SerializeField]
    private GameObject gasVisual;

    [SerializeField]
    private bool startActive = false;


    [Header("Sound")]

    [SerializeField]
    private GasTrapSoundController
        gasSoundController;


    private bool isActive;

    private Collider gasCollider;


    private readonly Dictionary<IDamageable, Coroutine>
        damageCoroutines =
            new Dictionary<IDamageable, Coroutine>();


    private void Awake()
    {
        gasCollider =
            GetComponent<Collider>();


        if (gasSoundController == null)
        {
            gasSoundController =
                GetComponent<GasTrapSoundController>();
        }


        isActive =
            startActive;


        if (gasVisual != null)
        {
            gasVisual.SetActive(
                isActive
            );
        }


        if (
            isActive &&
            gasSoundController != null
        )
        {
            gasSoundController
                .StartGasSound();
        }
    }


    public void ActivateGas()
    {
        if (isActive)
            return;


        isActive =
            true;


        if (gasVisual != null)
        {
            gasVisual.SetActive(
                true
            );
        }


        // ==========================================
        // GAS SOUND
        // ==========================================

        if (gasSoundController != null)
        {
            gasSoundController
                .StartGasSound();
        }


        CheckExistingTargets();


        Debug.Log(
            "[GasDamageZone] Gas activated."
        );
    }


    public void DeactivateGas()
    {
        if (!isActive)
            return;


        isActive =
            false;


        if (gasVisual != null)
        {
            gasVisual.SetActive(
                false
            );
        }


        // ==========================================
        // GAS SOUND
        // ==========================================

        if (gasSoundController != null)
        {
            gasSoundController
                .StopGasSound();
        }


        StopAllDamage();


        Debug.Log(
            "[GasDamageZone] Gas deactivated."
        );
    }


    private void OnTriggerEnter(
        Collider other)
    {
        if (!isActive)
            return;


        TryStartDamage(
            other
        );
    }


    private void OnTriggerExit(
        Collider other)
    {
        if (
            !IsTargetLayer(
                other.gameObject.layer
            )
        )
        {
            return;
        }


        IDamageable damageable =
            other.GetComponentInParent<
                IDamageable
            >();


        if (damageable == null)
            return;


        StopDamageRoutine(
            damageable
        );
    }


    private void CheckExistingTargets()
    {
        if (gasCollider == null)
            return;


        Bounds bounds =
            gasCollider.bounds;


        Collider[] colliders =
            Physics.OverlapBox(
                bounds.center,
                bounds.extents,
                Quaternion.identity,
                targetLayers,
                QueryTriggerInteraction.Ignore
            );


        foreach (
            Collider collider
            in colliders
        )
        {
            TryStartDamage(
                collider
            );
        }
    }


    private void TryStartDamage(
        Collider other)
    {
        if (
            !IsTargetLayer(
                other.gameObject.layer
            )
        )
        {
            return;
        }


        IDamageable damageable =
            other.GetComponentInParent<
                IDamageable
            >();


        if (damageable == null)
            return;


        if (
            damageCoroutines.ContainsKey(
                damageable
            )
        )
        {
            return;
        }


        Coroutine coroutine =
            StartCoroutine(
                DamageRoutine(
                    damageable
                )
            );


        damageCoroutines.Add(
            damageable,
            coroutine
        );
    }


    private IEnumerator DamageRoutine(
        IDamageable damageable)
    {
        while (isActive)
        {
            // Наносим урон
            damageable.TakeDamage(
                damage
            );


            // Если объект является Player,
            // проигрываем кашель
            Component component =
                damageable as Component;


            if (component != null)
            {
                PlayerVoiceController
                    playerVoiceController =
                        component
                            .GetComponent<
                                PlayerVoiceController>();


                if (
                    playerVoiceController ==
                    null
                )
                {
                    playerVoiceController =
                        component
                            .GetComponentInParent<
                                PlayerVoiceController>();
                }


                if (
                    playerVoiceController !=
                    null
                )
                {
                    playerVoiceController
                        .PlayCough();
                }
            }


            yield return new WaitForSeconds(
                damageInterval
            );


            if (
                !IsDamageableInsideZone(
                    damageable
                )
            )
            {
                break;
            }
        }


        StopDamageRoutine(
            damageable
        );
    }


    private bool IsDamageableInsideZone(
        IDamageable damageable)
    {
        if (
            !isActive ||
            gasCollider == null
        )
        {
            return false;
        }


        Component component =
            damageable as Component;


        if (component == null)
            return false;


        Collider targetCollider =
            component.GetComponentInParent<
                Collider>();


        if (targetCollider == null)
            return false;


        return Physics.ComputePenetration(
            gasCollider,
            gasCollider.transform.position,
            gasCollider.transform.rotation,
            targetCollider,
            targetCollider.transform.position,
            targetCollider.transform.rotation,
            out _,
            out _
        );
    }


    private void StopDamageRoutine(
        IDamageable damageable)
    {
        if (
            damageCoroutines.TryGetValue(
                damageable,
                out Coroutine coroutine
            )
        )
        {
            if (coroutine != null)
            {
                StopCoroutine(
                    coroutine
                );
            }
        }


        damageCoroutines.Remove(
            damageable
        );
    }


    private void StopAllDamage()
    {
        foreach (
            Coroutine coroutine
            in damageCoroutines.Values
        )
        {
            if (coroutine != null)
            {
                StopCoroutine(
                    coroutine
                );
            }
        }


        damageCoroutines.Clear();
    }


    private bool IsTargetLayer(
        int layer)
    {
        return (
            targetLayers.value &
            (1 << layer)
        ) != 0;
    }


    private void OnDisable()
    {
        StopAllDamage();
    }
}