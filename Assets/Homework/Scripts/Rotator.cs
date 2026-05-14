using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    private Vector3 _rotate = new Vector3(0f, 75f, 0f);
    private Rigidbody _rigidBody;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        if (_rigidBody == null)
        {
            return;
        }
        _rigidBody.isKinematic = true;
        StartCoroutine(Rotate());
    }

    private IEnumerator Rotate()
    {
        while (true)
        {
            _rigidBody.MoveRotation(_rigidBody.rotation *
            Quaternion.Euler(_rotate * Time.fixedDeltaTime));

            yield return new WaitForFixedUpdate();
        }
    }

}
