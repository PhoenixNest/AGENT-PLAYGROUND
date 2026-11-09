using Balls;
using DG.Tweening;
using Levels;
using TMPro;
using UnityEngine;

public class DivideGate : GateBase
{
	public Transform TextScale;

	public MeshRenderer VisualMeshRenderer;

	private int ballsCounter;

	private Sequence textSequence;

	private int divideEffectTargetCount;

	private int divideEffectCurrentCount;

	private Vector3 textScale;

	public override GateType Type { get; protected set; } = GateType.Divide;

	public override int Value { get; protected set; } = 2;

	public bool LastBallPassed { get; private set; }

	internal static bool DivisionHit(ref int counter, int divisor)
	{
		counter++;
		return divisor != 0 && counter % divisor == 0;
	}

	public override void SetValue(int value)
	{
		Value = value;
		divideEffectTargetCount = value;
	}

	protected override void Start()
	{
		if (VisualMeshRenderer != null && MaterialManager.instance != null)
		{
			Material material = MaterialManager.instance.GetMultiplicatorMaterial(-1);
			if (material != null)
			{
				VisualMeshRenderer.sharedMaterial = material;
			}
		}
		base.Start();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		textSequence?.Kill();
		textSequence = null;
	}

	public void Divide(BallView ball)
	{
		int counter = ballsCounter;
		bool hit = DivisionHit(ref counter, Value);
		ballsCounter = counter;
		LastBallPassed = !hit;

		if (!hit)
		{
			return;
		}

		LevelsController.SubtractTotal(BallsController.GetBallValue(ball));
		BallsController.Divide(ball);
		LevelsController.StageCheck();

		divideEffectTargetCount = Value;
		divideEffectCurrentCount = Value != 0 ? ballsCounter % Value : 0;
		Pulse();
		SpawnDivideEffect();
	}

	private void Pulse()
	{
		if (TextScale == null)
		{
			return;
		}
		if (textScale == Vector3.zero)
		{
			textScale = TextScale.localScale;
		}
		textSequence?.Kill();
		textSequence = DOTween.Sequence()
			.Append(TextScale.DOScale(textScale * 1.25f, 0.1f))
			.Append(TextScale.DOScale(textScale, 0.1f))
			.SetLink(gameObject);
	}

	private void SpawnDivideEffect()
	{
		GameManager game = GameManager.instance;
		if (game == null || game.DivideEffectPool == null)
		{
			return;
		}
		GameObject clone = game.DivideEffectPool.Spawn(transform.position, Quaternion.identity);
		DivideEffect effect = clone != null ? clone.GetComponent<DivideEffect>() : null;
		if (effect != null)
		{
			effect.SetText("/" + Value, divideEffectCurrentCount + "/" + divideEffectTargetCount);
		}
	}

	private void Update()
	{
	}
}
