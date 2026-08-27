using System.Collections;
using UnityEngine;

public class ImpactVfx : MonoBehaviour
{
    // =========================================================
    // COMPONENTS
    // =========================================================

    private ParticleSystem[] particleSystems;


    // =========================================================
    // STATE
    // =========================================================

    private Coroutine returnCoroutine;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        particleSystems =
            GetComponentsInChildren<ParticleSystem>(
                true
            );
    }


    // =========================================================
    // PLAY
    // =========================================================

    public void Play(
        Vector3 position,
        Quaternion rotation)
    {
        // =====================================================
        // STOP PREVIOUS RETURN
        // =====================================================

        if (returnCoroutine != null)
        {
            StopCoroutine(
                returnCoroutine
            );

            returnCoroutine =
                null;
        }


        // =====================================================
        // POSITION
        // =====================================================

        transform.SetPositionAndRotation(
            position,
            rotation
        );


        // =====================================================
        // ACTIVATE
        // =====================================================

        gameObject.SetActive(
            true
        );


        // =====================================================
        // RESET AND PLAY
        // =====================================================

        foreach (
            ParticleSystem particleSystem
            in particleSystems
        )
        {
            if (particleSystem == null)
                continue;


            particleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );


            particleSystem.Play(
                true
            );
        }


        // =====================================================
        // WAIT FOR FINISH
        // =====================================================

        returnCoroutine =
            StartCoroutine(
                ReturnWhenFinished()
            );
    }


    // =========================================================
    // WAIT FOR PARTICLES
    // =========================================================

    private IEnumerator ReturnWhenFinished()
    {
        yield return null;


        while (
            AreParticlesAlive()
        )
        {
            yield return null;
        }


        returnCoroutine =
            null;


        ImpactVfxPool.Instance?.Return(
            this
        );
    }


    // =========================================================
    // CHECK PARTICLES
    // =========================================================

    private bool AreParticlesAlive()
    {
        if (
            particleSystems == null ||
            particleSystems.Length == 0
        )
        {
            return false;
        }


        foreach (
            ParticleSystem particleSystem
            in particleSystems
        )
        {
            if (
                particleSystem != null &&
                particleSystem.IsAlive(
                    true
                )
            )
            {
                return true;
            }
        }


        return false;
    }


    // =========================================================
    // RETURN TO POOL
    // =========================================================

    public void ReturnToPool()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(
                returnCoroutine
            );

            returnCoroutine =
                null;
        }


        if (particleSystems != null)
        {
            foreach (
                ParticleSystem particleSystem
                in particleSystems
            )
            {
                if (particleSystem == null)
                    continue;


                particleSystem.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }
        }


        gameObject.SetActive(
            false
        );
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        returnCoroutine =
            null;
    }
}