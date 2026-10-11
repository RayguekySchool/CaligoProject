using UnityEngine;

public class DestroyParticles : MonoBehaviour
{
    private ParticleSystem particles;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        if (particles != null)
        {
            Destroy(gameObject, particles.main.duration
                + particles.main.startLifetime.constantMax);
        }
    }
}