using UnityEngine;

public interface IItemFinder
{
    Transform FindClosestItem(Vector3 position, float radius);
}