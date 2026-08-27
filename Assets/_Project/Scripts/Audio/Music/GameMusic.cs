using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class GameMusic : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip musicClip;

    [Header("Settings")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float volume = 1f;

    [Header("Crossfade")]
    [SerializeField] private float fadeInDuration = 1.5f;

    private AudioSource audioSource;
    private Tween fadeTween;

    public static GameMusic Instance { get; private set; }


    private void Awake()
    {
        // Защита от дубликатов
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ВАЖНО:
        // GameMusic должен быть корневым GameObject в Hierarchy,
        // а не дочерним объектом другого GameObject.
        DontDestroyOnLoad(gameObject);


        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;

        if (musicClip != null)
        {
            audioSource.clip = musicClip;
        }

        if (MusicTransitionManager.IsCrossfading &&
            !MusicTransitionManager.IsReturningToMenu)
        {
            audioSource.volume = 0f;
        }
        else
        {
            audioSource.volume = volume;
        }
    }


    private void Start()
    {
        if (!playOnStart)
            return;

        PlayMusic();

        if (MusicTransitionManager.IsCrossfading &&
            !MusicTransitionManager.IsReturningToMenu)
        {
            FadeIn(fadeInDuration);

            MusicTransitionManager.CompleteTransition();
        }
    }


    public void PlayMusic()
    {
        if (audioSource == null ||
            audioSource.clip == null)
        {
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }


    public void FadeIn(float duration)
    {
        if (audioSource == null)
            return;

        fadeTween?.Kill();

        audioSource.volume = 0f;

        fadeTween = audioSource
            .DOFade(volume, duration)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true);
    }


    public void FadeOutAndDestroy(float duration)
    {
        if (audioSource == null)
        {
            Destroy(gameObject);
            return;
        }

        fadeTween?.Kill();

        fadeTween = audioSource
            .DOFade(0f, duration)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (audioSource != null)
                {
                    audioSource.Stop();
                }

                Destroy(gameObject);
            });
    }


    public void StopMusic()
    {
        fadeTween?.Kill();

        if (audioSource != null &&
            audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }


    public void PauseMusic()
    {
        if (audioSource != null &&
            audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }


    public void ResumeMusic()
    {
        if (audioSource != null &&
            !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }


    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }


    private void OnDestroy()
    {
        fadeTween?.Kill();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}