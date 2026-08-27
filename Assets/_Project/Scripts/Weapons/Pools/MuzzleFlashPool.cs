using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlashPool : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static MuzzleFlashPool Instance
    {
        get;
        private set;
    }


    // =========================================================
    // POOL
    // =========================================================

    private class Pool
    {
        public Queue<PooledFlash> Available =
            new Queue<PooledFlash>();
    }


    // =========================================================
    // POOLED FLASH
    // =========================================================

    private class PooledFlash
    {
        public GameObject GameObject;

        public ParticleSystem[] ParticleSystems;
    }


    // =========================================================
    // POOLS
    // =========================================================

    private readonly Dictionary<int, Pool>
        pools =
            new Dictionary<int, Pool>();


    // =========================================================
    // UNITY
    // =========================================================

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


    // =========================================================
    // PLAY
    // =========================================================

    public void Play(
        ParticleSystem template,
        Transform muzzlePoint)
    {
        if (template == null)
            return;


        if (muzzlePoint == null)
            return;


        // =====================================================
        // GET POOL ID
        // =====================================================

        int poolId =
            template.GetInstanceID();


        // =====================================================
        // GET OR CREATE POOL
        // =====================================================

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


        // =====================================================
        // GET FLASH
        // =====================================================

        PooledFlash flash =
            GetFlash(
                template,
                pool
            );


        // =====================================================
        // SET POSITION
        // =====================================================

        flash.GameObject.transform.SetPositionAndRotation(
            muzzlePoint.position,
            muzzlePoint.rotation
        );


        // =====================================================
        // SET SCALE
        // =====================================================

        flash.GameObject.transform.localScale =
            muzzlePoint.lossyScale;


        // =====================================================
        // ACTIVATE
        // =====================================================

        flash.GameObject.SetActive(
            true
        );


        // =====================================================
        // PLAY PARTICLES
        // =====================================================

        PlayParticles(
            flash
        );


        // =====================================================
        // GET DURATION
        // =====================================================

        float duration =
            GetDuration(
                flash
            );


        // =====================================================
        // RETURN TO POOL
        // =====================================================

        StartCoroutine(
            ReturnAfterDelay(
                flash,
                pool,
                duration
            )
        );
    }


    // =========================================================
    // GET FLASH
    // =========================================================

    private PooledFlash GetFlash(
        ParticleSystem template,
        Pool pool)
    {
        // =====================================================
        // GET FROM POOL
        // =====================================================

        if (
            pool.Available.Count > 0
        )
        {
            return
                pool.Available.Dequeue();
        }


        // =====================================================
        // CREATE NEW
        // =====================================================

        GameObject flashObject =
            Instantiate(
                template.gameObject,
                transform
            );


        flashObject.name =
            template.name +
            "_Pooled";


        // =====================================================
        // DISABLE BEFORE USE
        // =====================================================

        flashObject.SetActive(
            false
        );


        // =====================================================
        // CREATE POOLED FLASH
        // =====================================================

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


    // =========================================================
    // PLAY PARTICLES
    // =========================================================

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


    // =========================================================
    // DURATION
    // =========================================================

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


    // =========================================================
    // RETURN TO POOL
    // =========================================================

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


        // =====================================================
        // STOP PARTICLES
        // =====================================================

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


        // =====================================================
        // DISABLE
        // =====================================================

        flash.GameObject.SetActive(
            false
        );


        // =====================================================
        // RETURN TO POOL
        // =====================================================

        pool.Available.Enqueue(
            flash
        );
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance =
                null;
        }
    }
}