using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyGizmo : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnDrawGizmos()
    {
        if (enemy == null)
            enemy = GetComponent<Enemy>();

        Vector3 center = enemy.ViewCenter;

        //Радиус обзора
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, enemy.ViewRadius);

        Gizmos.color = Color.red;

        Vector3 leftRay =
            Quaternion.AngleAxis(-enemy.ViewAngle / 2f, transform.up) *
            transform.forward *
            enemy.ViewRadius;

        Vector3 rightRay =
            Quaternion.AngleAxis(enemy.ViewAngle / 2f, transform.up) *
            transform.forward *
            enemy.ViewRadius;

        Gizmos.DrawRay(center, leftRay);
        Gizmos.DrawRay(center, rightRay);
    }
}
