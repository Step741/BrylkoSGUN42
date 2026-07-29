using UnityEngine;
using Zenject;

public class ItemFinder : IItemFinder
{
    private readonly LayerMask collectibleLayer;

    public ItemFinder(
        [Inject(Id = "CollectibleLayer")]
        LayerMask collectibleLayer)
    {
        this.collectibleLayer = collectibleLayer;
    }

    public Transform FindClosestItem(Vector3 position, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(position,radius,collectibleLayer);

        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hit.transform;
            }
        }

        return closest;
    }
}