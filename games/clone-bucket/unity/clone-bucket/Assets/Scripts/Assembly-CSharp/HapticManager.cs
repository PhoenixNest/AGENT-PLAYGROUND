using System;
using UnityEngine;

public class HapticManager : SingletonBehaviour<HapticManager>
{
	private const float MinPulseInterval = 0.05f;

	private const int PresetSelection = 0;

	private const int PresetSuccess = 1;

	private const int PresetWarning = 2;

	private const int PresetFailure = 3;

	private const int PresetLightImpact = 4;

	private const int PresetMediumImpact = 5;

	private const int PresetHeavyImpact = 6;

	[HideInInspector]
	public bool isHapticPlay = true;

	private float waitTimer;

	protected override void Awake()
	{
		base.Awake();
	}

	public void Reset()
	{
		waitTimer = 0f;
	}

	public void HapticOn()
	{
		isHapticPlay = true;
	}

	public void HapticOff()
	{
		isHapticPlay = false;
	}

	public virtual void LightImpact()
	{
		Pulse(PresetLightImpact);
	}

	public virtual void Selection()
	{
		Pulse(PresetSelection);
	}

	public virtual void MediumImpact()
	{
		Pulse(PresetMediumImpact);
	}

	public virtual void HeavyImpact()
	{
		Pulse(PresetHeavyImpact);
	}

	public virtual void Failure()
	{
		Pulse(PresetFailure);
	}

	public virtual void Success()
	{
		Pulse(PresetSuccess);
	}

	public virtual void Warning()
	{
		Pulse(PresetWarning);
	}

	public static bool CanPulse(float timer)
	{
		return timer >= MinPulseInterval;
	}

	public static float TickTimer(float timer, float deltaTime)
	{
		return timer + Mathf.Max(0f, deltaTime);
	}

	private void Pulse(int preset)
	{
		if (!isHapticPlay || !CanPulse(waitTimer))
		{
			return;
		}

		waitTimer = 0f;
		PlayPreset(preset);
	}

	private static void PlayPreset(int preset)
	{
		switch (preset)
		{
			case PresetSelection:
				Vibrate(6, 50);
				break;
			case PresetSuccess:
				Vibrate(15, 120);
				break;
			case PresetWarning:
				Vibrate(25, 200);
				break;
			case PresetFailure:
				Vibrate(45, 255);
				break;
			case PresetLightImpact:
				Vibrate(12, 80);
				break;
			case PresetMediumImpact:
				Vibrate(20, 140);
				break;
			case PresetHeavyImpact:
				Vibrate(35, 255);
				break;
		}
	}

	private void Update()
	{
		waitTimer = TickTimer(waitTimer, Time.deltaTime);
	}

	private static void Vibrate(int durationMs, int amplitude)
	{
#if UNITY_EDITOR
		return;
#elif UNITY_IOS
		Handheld.Vibrate();
#endif
	}
}
