using System.Collections;
using UnityEngine;

public class Garbage : MonoBehaviour
{
    [SerializeField]
    private float shrinkSpeed = 3f;

    private bool isCollected;

    public void Collect()
    {
        if (isCollected)
            return;

        isCollected = true;

        StartCoroutine(ShrinkRoutine());
    }

    private IEnumerator ShrinkRoutine()
    {
        Vector3 startScale = transform.localScale;

        while (transform.localScale.magnitude > 0.05f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.zero,
                shrinkSpeed * Time.deltaTime);

            yield return null;
        }

        Destroy(gameObject);
    }
}