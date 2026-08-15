using System.Collections;
using UnityEngine;

public class ElevatorPlatformTrigger : MonoBehaviour
{
    [SerializeField] private ElevatorController elevator;

    [Header("Return")]
    [SerializeField] private float returnDelay = 0.25f;

    private Coroutine returnCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        elevator.SetPlayer(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        elevator.ClearPlayer(other.transform);

        if (elevator.IsAtTop())
        {
            returnCoroutine = StartCoroutine(ReturnAfterDelay());
        }
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        elevator.ReturnDown();

        returnCoroutine = null;
    }

    private bool IsPlayer(Collider other)
    {
        return other.gameObject.layer == LayerMask.NameToLayer("Player");
    }
}