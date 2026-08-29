using UnityEngine;
using Zenject;

public class EnemyFactory : IEnemyFactory
{
    private readonly DiContainer container;

    public EnemyFactory(
        DiContainer container)
    {
        this.container = container;
    }

    public Enemy Create(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation
    )
    {
        if (prefab == null)
        {
            return null;
        }

        Enemy enemy =
            container.InstantiatePrefabForComponent<Enemy>(
                prefab,
                position,
                rotation,
                null
            );

        if (enemy == null)
        {
            return null;
        }
        return enemy;
    }
}