using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmbientZone : MonoBehaviour
{
    [Header("Ambient Controller")]

    [SerializeField]
    private AmbientSound ambientSound;


    [Header("Base Ambient")]

    [SerializeField]
    private AudioClip ambientClip;

    [Range(0f, 1f)]
    [SerializeField]
    private float ambientVolume = 1f;


    [Header("Random Ambient Sounds")]

    [SerializeField]
    private AudioClip[] randomAmbientClips;


    [Header("Transition")]

    [SerializeField]
    private float fadeDuration = 1.5f;


    [Header("Player Layer")]

    [SerializeField]
    private LayerMask playerLayer;


    private void Reset()
    {
        Collider zoneCollider =
            GetComponent<Collider>();

        zoneCollider.isTrigger = true;
    }


    private void OnTriggerEnter(
        Collider other
    )
    {
        // Проверяет слой объекта, который вошёл в Ambient Zone
        if (((1 << other.gameObject.layer) & playerLayer) == 0)
            return;


        if (ambientSound == null)
        {
            ambientSound =
                FindFirstObjectByType<AmbientSound>();
        }


        if (ambientSound == null)
        {
            Debug.LogWarning(
                "AmbientSound not found in scene."
            );

            return;
        }


        ambientSound.ChangeAmbient(
            ambientClip,
            ambientVolume,
            randomAmbientClips,
            fadeDuration
        );
    }
}