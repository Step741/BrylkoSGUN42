using UnityEngine;

public static class SurfaceImpactUtility
{
    public static void ProcessHit(
        RaycastHit hit)
    {
        if (hit.collider == null)
            return;


        SurfaceIdentifier surface =
            hit.collider.GetComponent<
                SurfaceIdentifier
            >();


        if (surface == null)
        {
            surface =
                hit.collider.GetComponentInParent<
                    SurfaceIdentifier
                >();
        }


        if (surface == null)
            return;


        surface.PlayImpact(
            hit.point,
            hit.normal,
            hit.collider.transform
        );
    }
}