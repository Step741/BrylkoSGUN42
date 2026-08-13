using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil")]
    [SerializeField]
    private float kickBack = 0.03f;

    [SerializeField]
    private float kickUp = 2f;

    [SerializeField]
    private float returnSpeed = 12f;

    [SerializeField]
    private float snappiness = 18f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Vector3 currentPosition;
    private Quaternion currentRotation;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        currentPosition = initialPosition;
        currentRotation = initialRotation;

        targetPosition = initialPosition;
        targetRotation = initialRotation;
    }

    private void Update()
    {
        targetPosition = Vector3.Lerp(
            targetPosition,
            initialPosition,
            returnSpeed * Time.deltaTime
        );

        targetRotation = Quaternion.Slerp(
            targetRotation,
            initialRotation,
            returnSpeed * Time.deltaTime
        );

        currentPosition = Vector3.Lerp(
            currentPosition,
            targetPosition,
            snappiness * Time.deltaTime
        );

        currentRotation = Quaternion.Slerp(
            currentRotation,
            targetRotation,
            snappiness * Time.deltaTime
        );

        transform.localPosition = currentPosition;
        transform.localRotation = currentRotation;
    }

    public void AddRecoil()
    {
        targetPosition -= Vector3.forward * kickBack;

        targetRotation *= Quaternion.Euler(
            -kickUp,
            0f,
            0f
        );
    }
}