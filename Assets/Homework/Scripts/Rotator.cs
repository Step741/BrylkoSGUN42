using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    private Vector3 _rotate = new Vector3(0f, 75f, 0f);
    private Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            return;
        }
        _rigidbody.isKinematic = true;
        StartCoroutine(Rotate());
    }

    private IEnumerator Rotate()
    {
        while (true)
        {
            _rigidbody.MoveRotation(_rigidbody.rotation *
            Quaternion.Euler(_rotate * Time.fixedDeltaTime));

            yield return new WaitForFixedUpdate();
        }
    }

}
