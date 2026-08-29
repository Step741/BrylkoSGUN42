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

        shell.Activate(
            ejectPoint.position,
            ejectPoint.rotation,
            shellLifetime
        );


        Rigidbody rb =
            shell.GetRigidbody();

        if (rb == null)
            return;

        rb.WakeUp();

        Vector3 force =
            ejectPoint.forward * ejectForce;

        force +=
            ejectPoint.up * upwardForce;

        force +=
            Random.insideUnitSphere *
            randomForce;

        rb.AddForce(
            force,
            ForceMode.Impulse
        );

        Vector3 torque =
            Random.insideUnitSphere *
            randomTorque;

        rb.AddTorque(
            torque,
            ForceMode.Impulse
        );
    }
}