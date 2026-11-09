using Lean.Pool;
using UnityEngine;

public class BucketCollectFX : MonoBehaviour, IPoolable
{
	public GameObject fx1;

	public ParticleSystem fx2;

	private SpriteRenderer fx1SR;

	public void OnSpawn()
	{
		if (fx1 != null)
		{
			fx1.SetActive(true);
			if (fx1SR == null)
			{
				fx1SR = fx1.GetComponent<SpriteRenderer>();
			}
			if (fx1SR != null)
			{
				fx1SR.enabled = true;
			}
		}
		if (fx2 != null)
		{
			fx2.Play(true);
		}
	}

	public void OnDespawn()
	{
		if (fx2 != null)
		{
			fx2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		if (fx1 != null)
		{
			fx1.SetActive(false);
		}
	}
}
