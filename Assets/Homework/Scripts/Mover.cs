using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField]
    private Vector3 _start;

    [SerializeField]
    private Vector3 _end;

    [SerializeField]
    private float _speed = 2f;

    [SerializeField]
    private float _delay = 0.5f;

    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody == null)
        {
            return;
        }

        transform.position = _start;
        StartCoroutine(MoveLoop());
    }

    private IEnumerator MoveLoop()
    {
        while (true)
        {
            yield return MoveToTarget(_end);

            yield return new WaitForSeconds(_delay);

            yield return MoveToTarget(_start);

            yield return new WaitForSeconds(_delay);
        }
    }

    private IEnumerator MoveToTarget(Vector3 _target)
    {
        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, _target);

        if (distance < 0.1f)
            yield break;

        float duration = distance / _speed;
        float passTime = 0f;

        while (passTime < duration)
        {
            passTime += Time.fixedDeltaTime;
            float time = Mathf.Clamp01(passTime / duration);

            Vector3 newPosition = Vector3.Lerp(start, _target, time);
            _rigidbody.MovePosition(newPosition);

            yield return new WaitForFixedUpdate();
        }

        _rigidbody.MovePosition(_target);
    }
}