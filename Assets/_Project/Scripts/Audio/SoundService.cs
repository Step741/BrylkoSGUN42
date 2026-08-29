using UnityEngine;
using UnityEngine.Audio;

public class SoundService : MonoBehaviour, ISoundService
{
    public static SoundService Instance { get; private set; }


    [Header("Audio Mixer Groups")]

    [SerializeField]
    private AudioMixerGroup musicMixerGroup;

    [SerializeField]
    private AudioMixerGroup sfxMixerGroup;

    [SerializeField]
    private AudioMixerGroup uiMixerGroup;

    [SerializeField]
    private AudioMixerGroup ambientMixerGroup;


    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(
                gameObject
            );

            return;
        }


        Instance = this;
    }


    public void Play2D(
        AudioClip clip,
        SoundType soundType = SoundType.SFX,
        float volume = 1f,
        float pitch = 1f)
    {
        if (clip == null)
            return;


        if (
            AudioSourcePool.Instance ==
            null
        )
        {
            return;
        }


        AudioSource source =
            AudioSourcePool.Instance.Get();


        source.transform.position =
            Vector3.zero;


        source.clip =
            clip;


        source.volume =
            volume;


        source.pitch =
            pitch;


        source.spatialBlend =
            0f;


        source.outputAudioMixerGroup =
            GetMixerGroup(
                soundType
            );


        source.Play();


        float duration =
            clip.length /
            Mathf.Abs(
                pitch
            );


        AudioSourcePool.Instance.ReleaseAfter(
            source,
            duration
        );
    }


    public void Play3D(
        AudioClip clip,
        Vector3 position,
        SoundType soundType = SoundType.SFX,
        float volume = 1f,
        float pitch = 1f,
        float minDistance = 3f,
        float maxDistance = 25f)
    {
        if (clip == null)
            return;


        if (
            AudioSourcePool.Instance ==
            null
        )
        {
            return;
        }


        AudioSource source =
            AudioSourcePool.Instance.Get();


        source.transform.position =
            position;


        source.clip =
            clip;


        source.volume =
            volume;


        source.pitch =
            pitch;

        source.spatialBlend =
            1f;


        source.minDistance =
            minDistance;


        source.maxDistance =
            maxDistance;


        source.outputAudioMixerGroup =
            GetMixerGroup(
                soundType
            );


        source.Play();


        float duration =
            clip.length /
            Mathf.Abs(
                pitch
            );


        AudioSourcePool.Instance.ReleaseAfter(
            source,
            duration
        );
    }


    private AudioMixerGroup GetMixerGroup(
        SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.Music:

                return musicMixerGroup;


            case SoundType.UI:

                return uiMixerGroup;


            case SoundType.Ambient:

                return ambientMixerGroup;


            case SoundType.SFX:

            default:

                return sfxMixerGroup;
        }
    }
}