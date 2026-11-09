using DG.Tweening;
using TMPro;
using UnityEngine;

public class Multiplicator : GateBase
{
	public TextMeshPro Text;

	public MeshRenderer visualMeshRenderer;

	private Sequence textSequence;

	private Vector3 textScale;

	public override GateType Type { get; protected set; } = GateType.Multiply;

	public override int Value { get; protected set; }

	internal static int MultiplyClones(int gateValue)
	{
		return Mathf.Max(0, gateValue - 1);
	}

	public override void SetValue(int value)
	{
		Value = value;
		UpdateValue();
	}

	internal static string ValueLabel(int value)
	{
		return "X" + value;
	}

	private void UpdateValue()
	{
		if (Text != null)
		{
			Text.text = ValueLabel(Value);
		}
	}

	private void Pulse()
	{
		if (Text == null)
		{
			return;
		}
		Transform t = Text.transform;
		if (textScale == Vector3.zero)
		{
			textScale = t.localScale;
		}
		textSequence?.Kill();
		textSequence = DOTween.Sequence()
			.Append(t.DOScale(textScale * 1.25f, 0.1f))
			.Append(t.DOScale(textScale, 0.1f))
			.SetLink(gameObject);
	}

	protected override void Start()
	{
		if (visualMeshRenderer != null && MaterialManager.instance != null)
		{
			Material material = MaterialManager.instance.GetMultiplicatorMaterial(Value - 1);
			if (material != null)
			{
				visualMeshRenderer.sharedMaterial = material;
			}
		}
		base.Start();
		UpdateValue();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		textSequence?.Kill();
		textSequence = null;
	}

	public void MultiEffect()
	{
		Pulse();
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayMultiplier();
		}
	}
}
