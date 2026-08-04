using UnityEngine;

public class WeaponAudio : MonoBehaviour
{
    [SerializeField]
    private AudioSource fireSource;

    [SerializeField]
    private AudioSource hitSource;

    [SerializeField]
    private AudioSource reloadSource;

    public void PlayFire()
    {
        if (fireSource != null && fireSource.clip != null)fireSource.PlayOneShot(fireSource.clip);
    }

    public void PlayHit()
    {
        if (hitSource != null && hitSource.clip != null)hitSource.PlayOneShot(hitSource.clip);
    }

    public void PlayReload()
    {
        if (reloadSource != null && reloadSource.clip != null)reloadSource.PlayOneShot(reloadSource.clip);
    }
}