using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class AudioSourcePool : MonoBehaviour
{
    public static AudioSourcePool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private int defaultCapacity = 8;
    [SerializeField] private int maxSize = 32;

    private ObjectPool<AudioSource> pool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        pool = new ObjectPool<AudioSource>(
            CreateAudioSource,
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyPoolObject,
            false,
            defaultCapacity,
            maxSize
        );
    }

    public AudioSource Get()
    {
        return pool.Get();
    }

    public void Release(AudioSource source)
    {
        if (source == null)
            return;

        pool.Release(source);
    }

    public void ReleaseAfter(
        AudioSource source,
        float duration)
    {
        StartCoroutine(
            ReleaseAfterCoroutine(
                source,
                duration
            )
        );
    }

    private AudioSource CreateAudioSource()
    {
        GameObject audioObject =
            new GameObject("PooledAudioSource");

        audioObject.transform.SetParent(transform);

        AudioSource source =
            audioObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.spatialBlend = 1f;

        return source;
    }

    private void OnTakeFromPool(AudioSource source)
    {
        source.gameObject.SetActive(true);
    }

    private void OnReturnToPool(AudioSource source)
    {
        source.Stop();

        source.clip = null;

        source.volume = 1f;
        source.pitch = 1f;

        source.loop = false;

        source.spatialBlend = 1f;

        source.outputAudioMixerGroup = null;

        source.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(AudioSource source)
    {
        if (source != null)
        {
            Destroy(source.gameObject);
        }
    }

    private IEnumerator ReleaseAfterCoroutine(
        AudioSource source,
        float duration)
    {
        yield return new WaitForSeconds(duration);

        if (source == null)
            yield break;

        if (source.isPlaying)
        {
            yield return new WaitWhile(
                () => source != null && source.isPlaying
            );
        }

        if (source != null)
        {
            pool.Release(source);
        }
    }
}