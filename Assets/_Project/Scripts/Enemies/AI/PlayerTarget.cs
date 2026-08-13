using UnityEngine;

public class PlayerTarget
{
    public Transform Transform { get; }

    public PlayerTarget(Transform transform)
    {
        Transform = transform;
    }
}