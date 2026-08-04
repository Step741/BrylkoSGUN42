using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DistanceAudio : MonoBehaviour
{
    [SerializeField]
    private Transform listener;

    [SerializeField]
    private float maxDistance = 20f;

    [SerializeField]
    private float nearPitch = 1.1f;

    [SerializeField]
    private float farPitch = 0.9f;

    [SerializeField]
    private float updateInterval = 0.1f;

    private AudioSource audioSource;
    private float timer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (listener == null)
            return;

        timer += Time.deltaTime;

        if (timer < updateInterval)
            return;

        timer = 0f;

        float distance = Vector3.Distance(transform.position, listener.position);
        float t = Mathf.Clamp01(distance / maxDistance);

        audioSource.pitch = Mathf.Lerp(nearPitch, farPitch, t);
    }
}