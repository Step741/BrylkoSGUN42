using UnityEngine;

public class FloatingSheep : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Скорость движения влево-вправо")]
    [SerializeField] private float speed = 2.0f;

    [Tooltip("Расстояние, на которое овца отходит от центра")]
    [SerializeField] private float amplitude = 0.3f;

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
        float newX = startPosition.x + Mathf.Cos((Time.time + timeOffset) * speed) * amplitude;

        //Обновляет позицию
        transform.position = new Vector3(newX, startPosition.y, startPosition.z);
    }
}