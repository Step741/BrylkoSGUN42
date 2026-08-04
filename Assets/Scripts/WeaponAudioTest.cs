using UnityEngine;

public class WeaponAudioTest : MonoBehaviour
{
    [SerializeField]
    private WeaponAudio weaponAudio;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            weaponAudio.PlayFire();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            weaponAudio.PlayHit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponAudio.PlayReload();
        }
    }
}