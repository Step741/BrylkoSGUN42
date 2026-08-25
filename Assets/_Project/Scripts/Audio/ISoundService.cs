using UnityEngine;

public interface ISoundService
{
    void Play2D(
        AudioClip clip,
        SoundType soundType = SoundType.SFX,
        float volume = 1f,
        float pitch = 1f
    );


    void Play3D(
        AudioClip clip,
        Vector3 position,
        SoundType soundType = SoundType.SFX,
        float volume = 1f,
        float pitch = 1f,
        float minDistance = 3f,
        float maxDistance = 25f
    );
}


public enum SoundType
{
    Music,
    SFX,
    UI,
    Ambient
}