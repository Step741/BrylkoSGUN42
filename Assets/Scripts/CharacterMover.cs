using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Renderer))]
public class CharacterMover : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField]
    private Transform[] waypoints;

    [Header("Movement")]
    [SerializeField]
    private float duration = 8f;

    [SerializeField]
    private float delay = 1f;

    [SerializeField]
    private Ease ease = Ease.Linear;

    [SerializeField]
    private LoopType loopType = LoopType.Restart;

    [SerializeField]
    [Tooltip("-1 = бесконечное повторение")]
    private int loops = -1;

    [Header("Scale Effect")]
    [SerializeField]
    private float scaleMultiplier = 1.2f;

    [SerializeField]
    private float scaleDuration = 0.4f;

    [Header("Color Effect")]
    [SerializeField]
    private Color moveColor = Color.cyan;

    [SerializeField]
    private float colorDuration = 0.5f;

    private Renderer objectRenderer;

    [Header("Dust Effect")]
    [SerializeField]
    private ParticleSystem dustEffect;

    [SerializeField]
    private float minDustEmission = 10f;

    [SerializeField]
    private float maxDustEmission = 40f;

    [SerializeField]
    private float dustPulseDuration = 0.5f;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        //Проверяем, что маршрут содержит минимум две точки
        if (waypoints == null || waypoints.Length < 2)
        {
            return;
        }

        PlayAnimation(CreatePath());
    }

    //Создаем массив координат из массива Transform
    private Vector3[] CreatePath()
    {
        Vector3[] path = new Vector3[waypoints.Length];

        for (int i = 0; i < waypoints.Length; i++)
        {
            path[i] = waypoints[i].position;
        }

        return path;
    }

    //Создаем последовательность анимаций DOTween
    private void PlayAnimation(Vector3[] path)
    {
        Sequence sequence = DOTween.Sequence();

        //Запускаем эффект пыли
        if (dustEffect != null)
        {
            dustEffect.Play();

            var emission = dustEffect.emission;

            DOTween.To(
                () => emission.rateOverTime.constant,
                value => emission.rateOverTime = value,
                maxDustEmission,
                dustPulseDuration
            )
            .SetLoops(-1, LoopType.Yoyo);
        }

        //Движение объекта по маршруту
        sequence.Append(
            transform.DOPath(path, duration, PathType.CatmullRom)
                .SetEase(ease)
        );

        //Одновременно увеличиваем и возвращаем масштаб
        sequence.Join(
            transform.DOScale(Vector3.one * scaleMultiplier, scaleDuration)
                .SetLoops(2, LoopType.Yoyo)
        );

        //Одновременно меняем цвет и возвращаем исходный
        sequence.Join(
            objectRenderer.material.DOColor(moveColor, colorDuration)
                .SetLoops(2, LoopType.Yoyo)
        );

        //Настройки последовательности
        sequence
            .SetDelay(delay)
            .SetLoops(loops, loopType);
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            //Рисуем точки маршрута
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);

            //Соединяем точки линиями
            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position,waypoints[i + 1].position);
            }
        }
    }
}