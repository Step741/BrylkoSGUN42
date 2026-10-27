using UnityEngine;

public class FloatingCloud : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Скорость покачивания облака")]
    [SerializeField] private float speed = 1.5f;

    [Tooltip("Максимальная высота движения вверх и вниз")]
    [SerializeField] private float amplitude = 0.2f;

    [Tooltip("Рассинхронность")]
    [SerializeField] private bool useRandomOffset = true;

    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        startPosition = transform.position;

        if (useRandomOffset)
        {
            timeOffset = Random.Range(0f, 100f);
        }
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * speed) * amplitude;

        //Обновляет позицию облака
        transform.position = new Vector3(transform.position.x, newY, startPosition.z);
    }
}