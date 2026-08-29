using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlashPool : MonoBehaviour
{
    public static MuzzleFlashPool Instance
    {
        get;
        private set;
    }

    private class Pool
    {
        public Queue<PooledFlash> Available =
            new Queue<PooledFlash>();
    }

    private class PooledFlash
    {
        public GameObject GameObject;

        public ParticleSystem[] ParticleSystems;
    }

    private readonly Dictionary<int, Pool>
        pools =
            new Dictionary<int, Pool>();

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(
                gameObject
            );

            return;
        }


        Instance =
            this;
    }

    public void Play(
        ParticleSystem template,
        Transform muzzlePoint)
    {
        if (template == null)
            return;


        if (muzzlePoint == null)
            return;

        int poolId =
            template.GetInstanceID();

        if (
            !pools.TryGetValue(
                poolId,
                out Pool pool
            )
        )
        {
            pool =
                new Pool();


            pools.Add(
                poolId,
                pool
            );
        }

        PooledFlash flash =
            GetFlash(
                template,
                pool
            );

        flash.GameObject.transform.SetPositionAndRotation(
            muzzlePoint.position,
            muzzlePoint.rotation
        );

        flash.GameObject.transform.localScale =
            muzzlePoint.lossyScale;

        flash.GameObject.SetActive(
            true
        );

        PlayParticles(
            flash
        );

        float duration =
            GetDuration(
                flash
            );

        StartCoroutine(
            ReturnAfterDelay(
                flash,
                pool,
                duration
            )
        );
    }

    private PooledFlash GetFlash(
        ParticleSystem template,
        Pool pool)
    {
        if (
            pool.Available.Count > 0
        )
        {
            return
                pool.Available.Dequeue();
        }

        GameObject flashObject =
            Instantiate(
                template.gameObject,
                transform
            );


        flashObject.name =
            template.name +
            "_Pooled";

        flashObject.SetActive(
            false
        );

        PooledFlash flash =
            new PooledFlash
            {
                GameObject =
                    flashObject,

                ParticleSystems =
                    flashObject.GetComponentsInChildren<
                        ParticleSystem
                    >(
                        true
                    )
            };


        return flash;
    }

    private void PlayParticles(
        PooledFlash flash)
    {
        foreach (
            ParticleSystem particle
            in flash.ParticleSystems
        )
        {
            if (particle == null)
                continue;


            particle.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );


            particle.Play(
                true
            );
        }
    }

    private float GetDuration(
        PooledFlash flash)
    {
        float longestDuration =
            0.1f;


        foreach (
            ParticleSystem particle
            in flash.ParticleSystems
        )
        {
            if (particle == null)
                continue;


            ParticleSystem.MainModule main =
                particle.main;


            float lifetime =
                main.startLifetime.constantMax;


            float duration =
                main.duration +
                lifetime;


            longestDuration =
                Mathf.Max(
                    longestDuration,
                    duration
                );
        }


        return
            longestDuration +
            0.05f;
    }

    private IEnumerator ReturnAfterDelay(
        PooledFlash flash,
        Pool pool,
        float delay)
    {
        yield return new WaitForSeconds(
            delay
        );


        if (
            flash == null ||
            flash.GameObject == null
        )
        {
            yield break;
        }

        foreach (
            ParticleSystem particle
            in flash.ParticleSystems
        )
        {
            if (particle == null)
                continue;


            particle.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }

        flash.GameObject.SetActive(
            false
        );

        pool.Available.Enqueue(
            flash
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance =
                null;
        }
    }
}