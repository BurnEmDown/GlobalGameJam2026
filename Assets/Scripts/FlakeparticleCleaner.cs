using UnityEngine;

public class FlakeparticleCleaner : MonoBehaviour
{
    [SerializeField] 
    private ParticleSystem flakesParticleSystem;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Clearing all flakes");
            ClearAllFlakes();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("Clearing some flakes");
            ClearSomeFlakes0();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Clearing some flakes");
            ClearSomeFlakes1();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Clearing some flakes");
            ClearSomeFlakes2();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Clearing some flakes");
            ClearSomeFlakes3();
        }
    }

    public void ClearSomeFlakes0()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[flakesParticleSystem.particleCount];
        int numParticles = flakesParticleSystem.GetParticles(particles);
        
        int aliveCount = 0;
        for (int i = 0; i < numParticles; i++)
        {
            // Keep only particles with more than 50% lifetime remaining (clear old ones)
            float lifetimePercent = particles[i].remainingLifetime / particles[i].startLifetime;
            if (lifetimePercent > 0.5f)
            {
                particles[aliveCount] = particles[i];
                aliveCount++;
            }
        }
        
        flakesParticleSystem.SetParticles(particles, aliveCount);
    }

    public void ClearSomeFlakes1()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[flakesParticleSystem.particleCount];
        int numParticles = flakesParticleSystem.GetParticles(particles);
        
        int aliveCount = 0;
        for (int i = 0; i < numParticles; i++)
        {
            // 50% chance to keep each particle
            if (Random.value > 0.5f)
            {
                particles[aliveCount] = particles[i];
                aliveCount++;
            }
        }
        
        flakesParticleSystem.SetParticles(particles, aliveCount);
    }

    public void ClearSomeFlakes2()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[flakesParticleSystem.particleCount];
        int numParticles = flakesParticleSystem.GetParticles(particles);
        
        // Remove 50% of particles (every other one)
        int particlesToKeep = Mathf.CeilToInt(numParticles * 0.5f);
        
        // Set the reduced particle array back
        flakesParticleSystem.SetParticles(particles, particlesToKeep);
    }

    public void ClearSomeFlakes3()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[flakesParticleSystem.particleCount];
        int numParticles = flakesParticleSystem.GetParticles(particles);
        
        int aliveCount = 0;
        for (int i = 0; i < numParticles; i++)
        {
            // 50% chance to keep each particle
            if (Random.value > 0.5f)
            {
                particles[aliveCount] = particles[i];
                aliveCount++;
            }
        }
        
        flakesParticleSystem.SetParticles(particles, aliveCount);
    }

    public void ClearAllFlakes()
    {
        flakesParticleSystem.Stop();
        flakesParticleSystem.Clear();
        flakesParticleSystem.Play();
    }
}
