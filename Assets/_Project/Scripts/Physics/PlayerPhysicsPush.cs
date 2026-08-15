using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerPhysicsPush : MonoBehaviour
{
    [Header("Push")]
    [SerializeField] private float pushForce = 3.5f;
    [SerializeField] private float maxPushMass = 50f;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null)
            return;

        if (rb.isKinematic)
            return;

        if (rb.mass > maxPushMass)
            return;

        Vector3 pushDirection = new Vector3(
            hit.moveDirection.x,
            0f,
            hit.moveDirection.z
        );

        if (pushDirection.sqrMagnitude < 0.01f)
            return;

        rb.AddForce(
            pushDirection.normalized * pushForce,
            ForceMode.Impulse
        );
    }
}