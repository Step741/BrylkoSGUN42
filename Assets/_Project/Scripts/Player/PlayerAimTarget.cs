using UnityEngine;

public class PlayerAimTarget : MonoBehaviour
{
    [SerializeField] private Transform aimPoint;

    public Transform AimPoint => aimPoint;
}