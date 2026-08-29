using System.Collections;
using UnityEngine;

public class ImpactVfx : MonoBehaviour
{
    private ParticleSystem[] particleSystems;

    private Coroutine returnCoroutine;

    private void Awake()
    {
        particleSystems =
            GetComponentsInChildren<ParticleSystem>(
                true
            );
    }

    public void Play(
        Vector3 position,
        Quaternion rotation)
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(
                returnCoroutine
            );

            returnCoroutine =
                null;
        }

        transform.SetPositionAndRotation(
            position,
            rotation
        );

        gameObject.SetActive(
            true
        );

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

        returnCoroutine =
            StartCoroutine(
                ReturnWhenFinished()
            );
    }

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

    private void OnDisable()
    {
        returnCoroutine =
            null;
    }
}