using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Pin : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private int ballLayer;
    private int pinLayer;

    private bool counted;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        ballLayer = LayerMask.NameToLayer("Ball");
        pinLayer = LayerMask.NameToLayer("Pin");
    }

    private void Update()
    {
        if (!GameManager.Instance.GameStarted)
            return;

        if (counted)
            return;

        float angle = Quaternion.Angle(startRotation, transform.rotation);

        if (angle > 45f)
        {
            counted = true;
            GameManager.Instance.PinKnockedDown();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!GameManager.Instance.GameStarted)
            return;

        int otherLayer = collision.gameObject.layer;

        if (otherLayer != ballLayer &&
            otherLayer != pinLayer)
            return;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void ResetPin()
    {
        counted = false;

        rb.isKinematic = true;

        transform.SetPositionAndRotation(startPosition, startRotation);

        rb.velocity = Vector3.zero;


        rb.angularVelocity = Vector3.zero;

        rb.Sleep();

        rb.isKinematic = false;
    }
}