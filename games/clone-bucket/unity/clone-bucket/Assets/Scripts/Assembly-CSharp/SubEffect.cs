using Lean.Pool;
using TMPro;
using UnityEngine;

public class SubEffect : MonoBehaviour, IPoolable
{
	public TextMeshPro Txt;

	public float EffectDuration;

	public float translateSpeed;

	private float timer;

	private Transform transformCache;

	private float posX;

	private float posZ;

	public void OnSpawn()
	{
		timer = 0f;
		posX = transform.position.x;
		posZ = transform.position.z;
		if (Txt != null)
		{
			Txt.enabled = true;
		}
	}

	public void OnDespawn()
	{
		if (Txt != null)
		{
			Txt.enabled = false;
		}
	}

	internal void SetText(string text)
	{
		if (Txt != null)
		{
			Txt.text = text;
		}
	}

	private void Start()
	{
		transformCache = transform;
	}

	private void Update()
	{
		Transform t = transformCache != null ? transformCache : transform;
		timer += Time.deltaTime;
		Vector3 p = t.position;
		t.position = new Vector3(posX, p.y + translateSpeed * Time.deltaTime, posZ);
		if (timer >= EffectDuration)
		{
			LeanPool.Despawn(gameObject);
		}
	}
}
