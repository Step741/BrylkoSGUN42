using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class AudioSourcePool : MonoBehaviour
{
    private static AudioSourcePool instance;

    private ObjectPool<AudioSource> pool;

    private void Awake()
    {
        if (instance != null &&
            instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        pool = new ObjectPool<AudioSource>(
            CreateAudioSource,

            source =>
            {
                source.gameObject.SetActive(true);
            },

            source =>
            {
                source.Stop();
                source.clip = null;
                source.gameObject.SetActive(false);
            },

            source =>
            {
                Destroy(source.gameObject);
            },

            false,
            8,
            32
        );
    }

    public static void Play(
        AudioClip clip,
        Vector3 position)
    {
        if (instance == null)
        {
            Debug.LogError(
                "AudioSourcePool is missing in scene."
            );

            return;
        }

        if (clip == null)
            return;

        AudioSource source =
            instance.pool.Get();

        source.transform.position =
            position;

        source.clip = clip;
        source.Play();

        instance.StartCoroutine(
            instance.ReleaseAfterPlay(
                source,
                clip.length
            )
        );
    }

    private AudioSource CreateAudioSource()
    {
        GameObject audioObject =
            new GameObject("PooledAudioSource");

        audioObject.transform.SetParent(
            transform
        );

        AudioSource source =
            audioObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.spatialBlend = 1f;

        return source;
    }

    private IEnumerator ReleaseAfterPlay(
        AudioSource source,
        float duration)
    {
        yield return new WaitForSeconds(duration);

        if (source == null)
            yield break;

        pool.Release(source);
    }
}