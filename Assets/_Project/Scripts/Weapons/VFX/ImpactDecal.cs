using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactDecal : MonoBehaviour
{
    private static readonly HashSet<ImpactDecal>
        activeDecals =
            new HashSet<ImpactDecal>();

    private Coroutine lifetimeCoroutine;

    private Transform followTarget;

    private Vector3 localPosition;

    private Quaternion localRotation;

    private bool wasFollowingTarget;

    public void Activate(
        Vector3 position,
        Quaternion rotation,
        float lifetime,
        Transform parent)
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(
                lifetimeCoroutine
            );

            lifetimeCoroutine =
                null;
        }

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

        activeDecals.Add(
            this
        );

        transform.SetPositionAndRotation(
            position,
            rotation
        );

        gameObject.SetActive(
            true
        );

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

    private void LateUpdate()
    {
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

        if (followTarget == null)
            return;

        transform.position =
            followTarget.TransformPoint(
                localPosition
            );

        transform.rotation =
            followTarget.rotation *
            localRotation;
    }

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

    public void ReturnToPool()
    {
        activeDecals.Remove(
            this
        );

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(
                lifetimeCoroutine
            );

            lifetimeCoroutine =
                null;
        }

        followTarget =
            null;

        wasFollowingTarget =
            false;

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

    //DISABLE
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

    // DESTROY
    private void OnDestroy()
    {
        activeDecals.Remove(
            this
        );
    }
}