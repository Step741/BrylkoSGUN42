using UnityEngine;
using UnityEngine.Audio;

public class GasTrapSoundController : MonoBehaviour
{
    [Header("Gas Sound")]

    [SerializeField]
    private AudioClip gasSound;


    [Header("Audio Source")]

    [SerializeField]
    private AudioSource audioSource;


    [Header("Mixer")]

    [SerializeField]
    private AudioMixerGroup sfxMixerGroup;


    [Header("Volume")]

    [SerializeField]
    [Range(0f, 1f)]
    private float volume = 1f;


    [Header("3D Sound Settings")]

    [SerializeField]
    private float minDistance = 5f;

    [SerializeField]
    private float maxDistance = 25f;


    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }


        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<
                    AudioSource
                >();
        }


        ConfigureAudioSource();
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
            return;


        audioSource.playOnAwake =
            false;


        audioSource.loop =
            true;


        audioSource.spatialBlend =
            1f;


        audioSource.minDistance =
            minDistance;


        audioSource.maxDistance =
            maxDistance;


        audioSource.volume =
            volume;


        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup =
                sfxMixerGroup;
        }
    }

    public void StartGasSound()
    {
        if (audioSource == null)
            return;

        if (gasSound == null)
            return;

        if (audioSource.isPlaying)
            return;


        audioSource.clip =
            gasSound;


        audioSource.Play();
    }


    public void StopGasSound()
    {
        if (audioSource == null)
            return;

        if (!audioSource.isPlaying)
            return;


        audioSource.Stop();
    }


    private void OnDisable()
    {
        StopGasSound();
    }
}