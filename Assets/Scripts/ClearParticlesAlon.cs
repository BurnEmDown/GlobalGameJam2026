using UnityEngine;
using System.Collections;

public class ClearParticlesAlon : MonoBehaviour
{
	public bool clear;
	
	private bool _clear;
	private ParticleSystem _particles;
	private ParticleSystem.Particle[] _particleList;
	
	void Start()
	{
		_particles = GetComponent<ParticleSystem>();
		_particleList = new ParticleSystem.Particle[_particles.main.maxParticles];
	}

    // Update is called once per frame
    void Update()
    {
        if (clear != _clear)
		{
			ClearParticles();
			_clear = clear;
		}
    }
	
	public void ClearParticles()
	{
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particles.main.maxParticles];
		int count = _particles.GetParticles(_particleList);

		for (int i = 0; i < count; i++)
		{
			bool keepParticle = Random.Range(0, 10) == 0;
			if (!keepParticle)
			{
				_particleList[i].velocity = Random.insideUnitCircle.normalized * Random.Range(10, 100);
			}
		}

		_particles.SetParticles(_particleList, count);
		StartCoroutine(StopParticles(count));
	}
	
	private IEnumerator StopParticles(int count)
	{
		yield return new WaitForSeconds(2);
				int count2 = _particles.GetParticles(_particleList);
    for (int i = 0; i < count; i++)
    {
		_particleList[i].velocity = Vector3.zero;
	}
	_particles.SetParticles(_particleList, count2);
	}
}
