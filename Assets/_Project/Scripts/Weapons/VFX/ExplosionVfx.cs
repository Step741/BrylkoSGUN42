using UnityEngine;

public class ExplosionVfx : MonoBehaviour
{
    private float lifetime;
    private float timer;

    public void Play(float duration)
    {
        lifetime = duration;
        timer = duration;

        gameObject.SetActive(true);

        ParticleSystem[] particles =
            GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particle in particles)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play(true);
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
}