using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

public class AmbientSound : MonoBehaviour
{
    [Header("Base Ambient")]

    [SerializeField]
    private AudioClip baseAmbientClip;

    [Range(0f, 1f)]
    [SerializeField]
    private float baseAmbientVolume = 1f;

    [SerializeField]
    private bool playBaseAmbientOnStart = true;


    [Header("Random Ambient Sounds")]

    [SerializeField]
    private AudioClip[] randomAmbientClips;

    [SerializeField]
    private bool playRandomSounds = true;

    [SerializeField]
    private float minRandomInterval = 8f;

    [SerializeField]
    private float maxRandomInterval = 20f;

    [Range(0f, 1f)]
    [SerializeField]
    private float randomVolumeMin = 0.6f;

    [Range(0f, 1f)]
    [SerializeField]
    private float randomVolumeMax = 1f;

    [SerializeField]
    private float randomPitchMin = 0.9f;

    [SerializeField]
    private float randomPitchMax = 1.1f;


    [Header("3D Random Sounds")]

    [SerializeField]
    private bool use3DRandomSounds = false;

    [SerializeField]
    private float minDistance = 5f;

    [SerializeField]
    private float maxDistance = 20f;


    [Header("Audio")]

    [SerializeField]
    private AudioMixerGroup ambientMixerGroup;


    private AudioSource baseAmbientSource;
    private Coroutine randomAmbientCoroutine;

    private Tween ambientTween;
    private int zoneVersion;


    // =========================
    // UNITY
    // =========================

    private void Awake()
    {
        CreateBaseAmbientSource();
    }


    private void Start()
    {
        if (playBaseAmbientOnStart)
        {
            PlayBaseAmbient();
        }

        StartRandomAmbientRoutine();
    }


    private void OnDestroy()
    {
        ambientTween?.Kill();

        if (randomAmbientCoroutine != null)
        {
            StopCoroutine(randomAmbientCoroutine);
        }
    }


    // =========================
    // BASE AMBIENT
    // =========================

    private void CreateBaseAmbientSource()
    {
        if (baseAmbientClip == null)
            return;

        baseAmbientSource =
            gameObject.AddComponent<AudioSource>();

        baseAmbientSource.playOnAwake = false;
        baseAmbientSource.loop = true;
        baseAmbientSource.clip = baseAmbientClip;
        baseAmbientSource.volume = baseAmbientVolume;
        baseAmbientSource.spatialBlend = 0f;

        baseAmbientSource.outputAudioMixerGroup =
            ambientMixerGroup;
    }


    public void PlayBaseAmbient()
    {
        if (baseAmbientSource == null)
            return;

        if (!baseAmbientSource.isPlaying)
        {
            baseAmbientSource.Play();
        }
    }


    public void StopBaseAmbient()
    {
        if (baseAmbientSource == null)
            return;

        ambientTween?.Kill();

        baseAmbientSource.Stop();
    }


    // =========================
    // ZONE CHANGE
    // =========================

    public void ChangeAmbient(
        AudioClip newBaseClip,
        float newBaseVolume,
        AudioClip[] newRandomClips,
        float fadeDuration = 1.5f
    )
    {
        zoneVersion++;

        int currentVersion = zoneVersion;

        baseAmbientVolume =
            Mathf.Clamp01(newBaseVolume);

        randomAmbientClips =
            newRandomClips;

        if (baseAmbientSource == null)
        {
            CreateAmbientSource(
                newBaseClip,
                0f
            );

            if (baseAmbientSource != null)
            {
                baseAmbientSource.Play();

                FadeToVolume(
                    baseAmbientVolume,
                    fadeDuration
                );
            }
        }
        else if (baseAmbientSource.clip == newBaseClip)
        {
            FadeToVolume(
                baseAmbientVolume,
                fadeDuration
            );
        }
        else
        {
            CrossfadeToNewClip(
                newBaseClip,
                baseAmbientVolume,
                fadeDuration,
                currentVersion
            );
        }

        RestartRandomAmbientRoutine();
    }


    private void CreateAmbientSource(
        AudioClip clip,
        float volume
    )
    {
        if (clip == null)
            return;

        baseAmbientSource =
            gameObject.AddComponent<AudioSource>();

        baseAmbientSource.playOnAwake = false;
        baseAmbientSource.loop = true;
        baseAmbientSource.clip = clip;
        baseAmbientSource.volume = volume;
        baseAmbientSource.spatialBlend = 0f;

        baseAmbientSource.outputAudioMixerGroup =
            ambientMixerGroup;
    }


    private void CrossfadeToNewClip(
        AudioClip newClip,
        float targetVolume,
        float duration,
        int currentVersion
    )
    {
        if (newClip == null)
        {
            FadeToVolume(
                0f,
                duration
            );

            return;
        }

        ambientTween?.Kill();

        AudioSource oldSource =
            baseAmbientSource;

        AudioSource newSource =
            gameObject.AddComponent<AudioSource>();

        newSource.playOnAwake = false;
        newSource.loop = true;
        newSource.clip = newClip;
        newSource.volume = 0f;
        newSource.spatialBlend = 0f;

        newSource.outputAudioMixerGroup =
            ambientMixerGroup;

        newSource.Play();

        Sequence sequence =
            DOTween.Sequence();

        sequence.Join(
            oldSource.DOFade(
                0f,
                duration
            )
        );

        sequence.Join(
            newSource.DOFade(
                targetVolume,
                duration
            )
        );

        sequence.SetUpdate(true);

        sequence.OnComplete(() =>
        {
            if (currentVersion != zoneVersion)
                return;

            if (oldSource != null)
            {
                oldSource.Stop();
                Destroy(oldSource);
            }

            baseAmbientSource =
                newSource;
        });

        ambientTween = sequence;
    }


    private void FadeToVolume(
        float targetVolume,
        float duration
    )
    {
        if (baseAmbientSource == null)
            return;

        ambientTween?.Kill();

        ambientTween =
            baseAmbientSource
                .DOFade(
                    targetVolume,
                    duration
                )
                .SetEase(Ease.InOutQuad)
                .SetUpdate(true);
    }


    // =========================
    // RANDOM AMBIENT
    // =========================

    private void StartRandomAmbientRoutine()
    {
        if (!playRandomSounds)
            return;

        if (randomAmbientClips == null ||
            randomAmbientClips.Length == 0)
        {
            return;
        }

        randomAmbientCoroutine =
            StartCoroutine(
                RandomAmbientRoutine()
            );
    }


    private void RestartRandomAmbientRoutine()
    {
        if (randomAmbientCoroutine != null)
        {
            StopCoroutine(
                randomAmbientCoroutine
            );

            randomAmbientCoroutine = null;
        }

        StartRandomAmbientRoutine();
    }


    private IEnumerator RandomAmbientRoutine()
    {
        while (true)
        {
            float delay =
                Random.Range(
                    minRandomInterval,
                    maxRandomInterval
                );

            yield return new WaitForSeconds(
                delay
            );

            PlayRandomAmbient();
        }
    }


    public void PlayRandomAmbient()
    {
        if (randomAmbientClips == null ||
            randomAmbientClips.Length == 0)
        {
            return;
        }

        AudioClip clip =
            randomAmbientClips[
                Random.Range(
                    0,
                    randomAmbientClips.Length
                )
            ];

        if (clip == null)
            return;

        float volume =
            Random.Range(
                randomVolumeMin,
                randomVolumeMax
            );

        float pitch =
            Random.Range(
                randomPitchMin,
                randomPitchMax
            );

        if (SoundService.Instance == null)
            return;

        if (use3DRandomSounds)
        {
            SoundService.Instance.Play3D(
                clip,
                GetRandomPosition(),
                SoundType.Ambient,
                volume,
                pitch
            );
        }
        else
        {
            SoundService.Instance.Play2D(
                clip,
                SoundType.Ambient,
                volume,
                pitch
            );
        }
    }


    private Vector3 GetRandomPosition()
    {
        Vector2 randomCircle =
            Random.insideUnitCircle.normalized;

        float distance =
            Random.Range(
                minDistance,
                maxDistance
            );

        Vector3 offset =
            new Vector3(
                randomCircle.x,
                0f,
                randomCircle.y
            ) * distance;

        return transform.position + offset;
    }
}