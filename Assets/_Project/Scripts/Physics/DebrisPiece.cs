using UnityEngine;

public class DebrisPiece : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private float explosionForce = 2.5f;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float upwardModifier = 0.4f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Explode(Vector3 explosionPosition)
    {
        if (rb == null)
            return;

        rb.AddExplosionForce(
            explosionForce,
            explosionPosition,
            explosionRadius,
            upwardModifier,
            ForceMode.Impulse
        );
    }
}