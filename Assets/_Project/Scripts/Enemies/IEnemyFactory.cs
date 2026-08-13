using UnityEngine;

public interface IEnemyFactory
{
    Enemy Create(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation
    );
}