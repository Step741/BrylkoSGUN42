using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Ball Type")]
    [SerializeField] private BallType ballType = BallType.Medium;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float moveLimit = 2f;

    private Rigidbody rb;

    public Rigidbody Rigidbody => rb;

    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        switch (ballType)
        {
            case BallType.Light:

                rb.mass = 7f;
                transform.localScale = Vector3.one * 0.6f;

                break;

            case BallType.Medium:

                rb.mass = 8f;
                transform.localScale = Vector3.one * 0.7f;

                break;

            case BallType.Heavy:

                rb.mass = 10f;
                transform.localScale = Vector3.one * 0.8f;

                break;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.GameStarted)
            return;

        MoveBall();
    }

    private void MoveBall()
    {
        Plane plane = new Plane(Vector3.up, transform.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);

            Vector3 position = transform.position;

            position.x = Mathf.Clamp(worldPoint.x, -moveLimit, moveLimit);

            transform.position = position;
        }
    }

    public void PlayRollingSound()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}