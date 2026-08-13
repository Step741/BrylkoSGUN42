using UnityEngine;

public class EnemyPatrolPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawSphere(
            transform.position,
            0.25f
        );
    }
}