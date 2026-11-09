using Lean.Pool;
using TMPro;
using UnityEngine;

public class DivideEffect : MonoBehaviour, IPoolable
{
	public TextMeshPro Txt1;

	public TextMeshPro Txt2;

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
		SetLabelsVisible(true);
	}

	public void OnDespawn()
	{
		SetLabelsVisible(false);
	}

	internal void SetText(string first, string second)
	{
		if (Txt1 != null)
		{
			Txt1.text = first;
		}
		if (Txt2 != null)
		{
			Txt2.text = second;
		}
	}

	private void SetLabelsVisible(bool visible)
	{
		if (Txt1 != null)
		{
			Txt1.enabled = visible;
		}
		if (Txt2 != null)
		{
			Txt2.enabled = visible;
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
