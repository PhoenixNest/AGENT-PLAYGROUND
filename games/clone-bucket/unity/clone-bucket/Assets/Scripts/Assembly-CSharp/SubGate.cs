using TMPro;
using UnityEngine;

public class SubGate : GateBase
{
	public int startValue;

	public TextMeshPro ValueText;

	public MeshRenderer visual;

	public new BoxCollider collider;

	private Transform transformCache;

	private int subEffectTargetCount;

	private int subEffectCurrentCount;

	public override GateType Type { get; protected set; } = GateType.Subtract;

	public override int Value { get; protected set; }

	internal static bool SubtractConsumes(ref int remaining)
	{
		if (remaining <= 0)
		{
			return false;
		}
		remaining--;
		return true;
	}

	public override void SetValue(int value)
	{
		Value = value;
		startValue = value;
		subEffectTargetCount = value;
		UpdateValue();
	}

	public void ResetGate()
	{
		if (visual != null)
		{
			visual.enabled = true;
		}
		if (collider != null)
		{
			collider.enabled = true;
		}
		if (ValueText != null)
		{
			ValueText.enabled = true;
		}
		subEffectCurrentCount = 0;
		subEffectTargetCount = startValue;
		SetRemaining(startValue);
	}

	private void SetRemaining(int remaining)
	{
		Value = remaining;
		UpdateValue();
	}

	internal static string ValueLabel(int value)
	{
		return "-" + value;
	}

	private void UpdateValue()
	{
		if (ValueText != null)
		{
			ValueText.text = ValueLabel(Value);
		}
	}

	public bool Sub()
	{
		int remaining = Value;
		if (!SubtractConsumes(ref remaining))
		{
			if (visual != null)
			{
				visual.enabled = false;
			}
			if (collider != null)
			{
				collider.enabled = false;
			}
			if (ValueText != null)
			{
				ValueText.enabled = false;
			}
			return false;
		}

		SetRemaining(remaining);
		subEffectCurrentCount++;
		return true;
	}

	private void SpawnSubEffect()
	{
		GameManager game = GameManager.instance;
		if (game == null || game.SubEffectPool == null)
		{
			return;
		}
		GameObject clone = game.SubEffectPool.Spawn(transform.position, Quaternion.identity);
		SubEffect effect = clone != null ? clone.GetComponent<SubEffect>() : null;
		if (effect != null)
		{
			effect.SetText("-" + startValue);
		}
	}

	private void Update()
	{
	}
}
