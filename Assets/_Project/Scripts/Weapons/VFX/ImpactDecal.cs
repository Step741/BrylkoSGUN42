using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactDecal : MonoBehaviour
{
    // =========================================================
    // ACTIVE DECALS
    // =========================================================

    private static readonly HashSet<ImpactDecal>
        activeDecals =
            new HashSet<ImpactDecal>();


    // =========================================================
    // STATE
    // =========================================================

    private Coroutine lifetimeCoroutine;


    // =========================================================
    // FOLLOW TARGET
    // =========================================================

    private Transform followTarget;

    private Vector3 localPosition;

    private Quaternion localRotation;

    private bool wasFollowingTarget;


    // =========================================================
    // SETUP
    // =========================================================

    public void Activate(
        Vector3 position,
        Quaternion rotation,
        float lifetime,
        Transform parent)
    {
        // =====================================================
        // STOP PREVIOUS TIMER
        // =====================================================

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(
                lifetimeCoroutine
            );

            lifetimeCoroutine =
                null;
        }


        // =====================================================
        // FOLLOW TARGET
        // =====================================================

        followTarget =
            parent;

        wasFollowingTarget =
            followTarget != null;


        if (followTarget != null)
        {
            localPosition =
                followTarget.InverseTransformPoint(
                    position
                );

            localRotation =
                Quaternion.Inverse(
                    followTarget.rotation
                ) *
                rotation;
        }


        // =====================================================
        // REGISTER ACTIVE DECAL
        // =====================================================

        activeDecals.Add(
            this
        );


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
        // LIFETIME
        // =====================================================

        if (lifetime > 0f)
        {
            lifetimeCoroutine =
                StartCoroutine(
                    ReturnAfterLifetime(
                        lifetime
                    )
                );
        }
    }


    // =========================================================
    // REMOVE DECALS FOR TARGET
    // =========================================================

    public static void RemoveDecalsForTarget(
        Transform target)
    {
        if (target == null)
            return;


        List<ImpactDecal> decalsToRemove =
            new List<ImpactDecal>();


        foreach (
            ImpactDecal decal
            in activeDecals
        )
        {
            if (decal == null)
                continue;


            if (
                decal.followTarget == null
            )
            {
                continue;
            }


            // =================================================
            // SAME OBJECT
            // =================================================

            if (
                decal.followTarget ==
                target
            )
            {
                decalsToRemove.Add(
                    decal
                );

                continue;
            }


            // =================================================
            // TARGET IS CHILD OF ENEMY
            // =================================================

            if (
                decal.followTarget.IsChildOf(
                    target
                )
            )
            {
                decalsToRemove.Add(
                    decal
                );
            }
        }


        // =====================================================
        // RETURN TO POOL
        // =====================================================

        foreach (
            ImpactDecal decal
            in decalsToRemove
        )
        {
            if (decal == null)
                continue;


            DecalPool.Instance?.Return(
                decal
            );
        }
    }


    // =========================================================
    // FOLLOW TARGET
    // =========================================================

    private void LateUpdate()
    {
        // =====================================================
        // TARGET DESTROYED
        // =====================================================

        if (
            wasFollowingTarget &&
            followTarget == null
        )
        {
            wasFollowingTarget =
                false;

            DecalPool.Instance?.Return(
                this
            );

            return;
        }


        // =====================================================
        // NO TARGET
        // =====================================================

        if (followTarget == null)
            return;


        // =====================================================
        // FOLLOW POSITION
        // =====================================================

        transform.position =
            followTarget.TransformPoint(
                localPosition
            );


        // =====================================================
        // FOLLOW ROTATION
        // =====================================================

        transform.rotation =
            followTarget.rotation *
            localRotation;
    }


    // =========================================================
    // RETURN AFTER LIFETIME
    // =========================================================

    private IEnumerator ReturnAfterLifetime(
        float lifetime)
    {
        yield return new WaitForSeconds(
            lifetime
        );


        lifetimeCoroutine =
            null;


        DecalPool.Instance?.Return(
            this
        );
    }


    // =========================================================
    // RETURN TO POOL
    // =========================================================

    public void ReturnToPool()
    {
        // =====================================================
        // REMOVE FROM ACTIVE LIST
        // =====================================================

        activeDecals.Remove(
            this
        );


        // =====================================================
        // STOP TIMER
        // =====================================================

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(
                lifetimeCoroutine
            );

            lifetimeCoroutine =
                null;
        }


        // =====================================================
        // STOP FOLLOWING
        // =====================================================

        followTarget =
            null;

        wasFollowingTarget =
            false;


        // =====================================================
        // RETURN UNDER POOL
        // =====================================================

        if (DecalPool.Instance != null)
        {
            transform.SetParent(
                DecalPool.Instance.transform,
                false
            );
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
        activeDecals.Remove(
            this
        );


        lifetimeCoroutine =
            null;

        followTarget =
            null;

        wasFollowingTarget =
            false;
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        activeDecals.Remove(
            this
        );
    }
}