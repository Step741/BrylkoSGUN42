using UnityEngine;

public class GasLightFlicker : MonoBehaviour
{
    [SerializeField] private Light gasLight;

    [Header("Intensity")]
    [SerializeField] private float minIntensity = 0.35f;
    [SerializeField] private float maxIntensity = 0.6f;

    [Header("Speed")]
    [SerializeField] private float speed = 2f;

    private void Awake()
    {
        if (gasLight == null)
            gasLight = GetComponent<Light>();
    }

    private void Update()
    {
        if (gasLight == null)
            return;

        float noise =
            Mathf.PerlinNoise(
                Time.time * speed,
                0f
            );

        gasLight.intensity =
            Mathf.Lerp(
                minIntensity,
                maxIntensity,
                noise
            );
    }
}