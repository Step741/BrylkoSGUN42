using UnityEngine;

public class ShellEjector : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ShellPool shellPool;

    [SerializeField]
    private Transform ejectPoint;


    [Header("Ejection")]
    [SerializeField]
    private float ejectForce = 2.5f;

    [SerializeField]
    private float upwardForce = 0.8f;

    [SerializeField]
    private float randomForce = 0.3f;

    [SerializeField]
    private float randomTorque = 5f;


    [Header("Lifetime")]
    [SerializeField]
    private float shellLifetime = 8f;


    public void Eject()
    {
        if (shellPool == null)
            return;

        if (ejectPoint == null)
            return;


        ShellCasing shell =
            shellPool.GetShell(
                ejectPoint.position,
                ejectPoint.rotation
            );

        if (shell == null)
            return;


        // Полностью подготавливаем гильзу
        shell.Activate(
            ejectPoint.position,
            ejectPoint.rotation,
            shellLifetime
        );


        Rigidbody rb =
            shell.GetRigidbody();

        if (rb == null)
            return;


        // На всякий случай будим Rigidbody
        rb.WakeUp();


        // Основное направление выброса
        Vector3 force =
            ejectPoint.forward * ejectForce;

        // Немного вверх
        force +=
            ejectPoint.up * upwardForce;

        // Случайный разброс
        force +=
            Random.insideUnitSphere *
            randomForce;


        rb.AddForce(
            force,
            ForceMode.Impulse
        );


        // Случайное вращение
        Vector3 torque =
            Random.insideUnitSphere *
            randomTorque;


        rb.AddTorque(
            torque,
            ForceMode.Impulse
        );
    }
}