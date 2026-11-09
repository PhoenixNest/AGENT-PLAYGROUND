using System;
using System.Collections.Generic;
using Home.Game.Runtime.EntryPoint.Mazes;
using Levels;
using UnityEngine;

public class GatesController : MonoBehaviour
{
	private const float SpikeXThreshold = 0.5f;

	[SerializeField]
	private GatesConfig _config;

	private static readonly List<GateBase> _gates = new List<GateBase>();

	private static readonly Dictionary<Transform, Multiplicator> _multiplicators = new Dictionary<Transform, Multiplicator>();

	private static readonly Dictionary<Transform, SubGate> _subGates = new Dictionary<Transform, SubGate>();

	private static readonly Dictionary<Transform, DivideGate> _divideGates = new Dictionary<Transform, DivideGate>();

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		ResetSubGates();
	}

	private void Update()
	{
		MoveGates();
		CheckGates();
	}

	private void MoveGates()
	{
		for (int i = 0; i < _gates.Count; i++)
		{
			GateBase gate = _gates[i];
			if (gate == null || !gate.isMoveX || gate.stopped)
			{
				continue;
			}
			if (!gate.IsMoving)
			{
				gate.Move();
			}
		}
	}

	public void ResetSubGates()
	{
		for (int i = 0; i < _gates.Count; i++)
		{
			SubGate sub = _gates[i] as SubGate;
			if (sub != null)
			{
				sub.ResetGate();
			}
		}
	}

	private void CheckGates()
	{
		for (int i = _gates.Count - 1; i >= 0; i--)
		{
			if (_gates[i] == null)
			{
				_gates.RemoveAt(i);
			}
		}

		ReconcileGates(LevelsController.MiniLevelView, LevelsController.CurrentMiniLevelData);
	}

	internal void ReconcileGates(MiniLevelView view, MiniLevelData mini)
	{
		if (view == null || view.allGates == null || mini.gateTypes == null)
		{
			return;
		}

		for (int i = 0; i < view.allGates.Length; i++)
		{
			GateBase gate = view.allGates[i];
			if (gate == null || i >= mini.gateTypes.Count)
			{
				continue;
			}

			if (NeedsRespawn(gate.Type, mini.gateTypes[i]))
			{
				gate = RespawnGate(view, i, mini.gateTypes[i]);
				if (gate == null)
				{
					continue;
				}
				ApplyConfiguredValue(gate, mini, i);
			}
		}
	}

	private static void ApplyConfiguredValue(GateBase gate, MiniLevelData mini, int index)
	{
		if (mini.gateValues != null && index < mini.gateValues.Count)
		{
			gate.SetValue(mini.gateValues[index]);
		}
	}

	internal static bool NeedsRespawn(GateType current, GateType rowType)
	{
		return current != rowType;
	}

	private GateBase RespawnGate(MiniLevelView view, int index, GateType type)
	{
		GateBase old = view.allGates[index];
		Transform oldTransform = old.transform;
		GateBase fresh = SpawnGate(type, oldTransform.position, oldTransform.rotation, oldTransform.localScale, oldTransform.parent);
		if (fresh == null)
		{
			return null;
		}

		fresh.stopped = old.stopped;
		view.allGates[index] = fresh;
		Destroy(old.gameObject);
		return fresh;
	}

	private static int GetGateRollSeed(string miniLevelName, int gateIndex)
	{
		unchecked
		{
			string name = miniLevelName ?? string.Empty;
			int h;
			if (name.Length < 1)
			{
				h = 0x20F;
			}
			else
			{
				h = 0x11;
				for (int i = 0; i < name.Length; i++)
				{
					h = h * 31 + name[i];
				}
				h *= 31;
			}

			const int state = 0;
			return gateIndex + (state + h) * 31;
		}
	}

	private static int CalculateMaxPossibleBalls(int startBallAmount, IReadOnlyList<GateBase> gates, MazeGateDefinition[] gateDefinitions)
	{
		float balls = startBallAmount;
		int i = 0;
		while (i < gates.Count)
		{
			GateBase current = gates[i];
			if (i + 1 < gates.Count && Mathf.Abs(PositionX(i, gates, gateDefinitions) - PositionX(i + 1, gates, gateDefinitions)) < SpikeXThreshold)
			{
				GateBase chosen = ChooseSpikeGate(balls, current, gates[i + 1]);
				balls = ApplyGate(balls, chosen);
				i += 2;
			}
			else
			{
				balls = ApplyGate(balls, current);
				i++;
			}
		}
		int rounded = (int)Math.Round(balls, MidpointRounding.ToEven);
		return Math.Max(0, rounded);
	}

	private static float PositionX(int index, IReadOnlyList<GateBase> gates, MazeGateDefinition[] gateDefinitions)
	{
		if (gateDefinitions != null && index < gateDefinitions.Length)
		{
			return gateDefinitions[index].positionX;
		}
		GateBase gate = gates[index];
		return gate != null ? gate.transform.localPosition.x : 0f;
	}

	private static GateBase ChooseSpikeGate(float currentBalls, GateBase a, GateBase b)
	{
		if (a == null)
		{
			return b;
		}
		if (b == null)
		{
			return a;
		}

		bool aMultiplies = a.Type == GateType.Multiply;
		bool bMultiplies = b.Type == GateType.Multiply;
		if (aMultiplies && bMultiplies)
		{
			return b.Value > a.Value ? b : a;
		}
		if (aMultiplies != bMultiplies)
		{
			return aMultiplies ? a : b;
		}
		return ApplyGate(currentBalls, a) < ApplyGate(currentBalls, b) ? b : a;
	}

	private static float ApplyGate(float balls, GateBase gate)
	{
		return ApplyGateValue(balls, gate.Type, gate.Value);
	}

	internal static float ApplyGateValue(float balls, GateType type, int value)
	{
		switch (type)
		{
			case GateType.Multiply:
				return balls * value;
			case GateType.Subtract:
				return Mathf.Max(0f, balls - value);
			case GateType.Divide:
				return value != 0 ? balls / value : balls;
			default:
				return balls;
		}
	}

	public GateBase SpawnGate(GateType type, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)
	{
		GateBase prefab = GetPrefab(type);
		if (prefab == null)
		{
			Debug.LogError("[MM3:Gates] no prefab in GatesConfig for " + type);
			return null;
		}
		GateBase gate = Instantiate(prefab, position, rotation, parent);
		gate.transform.localScale = scale;
		return gate;
	}

	private GateBase GetPrefab(GateType type)
	{
		if (_config == null)
		{
			return null;
		}
		switch (type)
		{
			case GateType.Multiply:
				return _config.multiplicationGatePrefab;
			case GateType.Subtract:
				return _config.substractionGatePrefab;
			case GateType.Divide:
				return _config.dividerGatePrefab;
			default:
				return null;
		}
	}

	internal static void Register(GateBase gate)
	{
		if (gate == null || _gates.Contains(gate))
		{
			return;
		}
		_gates.Add(gate);
		Collider[] colliders = gate.GetComponentsInChildren<Collider>(true);
		for (int i = 0; i < colliders.Length; i++)
		{
			Transform key = colliders[i].transform;
			Multiplicator multiplicator = gate as Multiplicator;
			SubGate subGate = gate as SubGate;
			DivideGate divideGate = gate as DivideGate;
			if (multiplicator != null)
			{
				_multiplicators[key] = multiplicator;
			}
			else if (subGate != null)
			{
				_subGates[key] = subGate;
			}
			else if (divideGate != null)
			{
				_divideGates[key] = divideGate;
			}
		}
	}

	internal static void Unregister(GateBase gate)
	{
		if (gate == null)
		{
			return;
		}
		_gates.Remove(gate);
		RemoveByValue(_multiplicators, gate);
		RemoveByValue(_subGates, gate);
		RemoveByValue(_divideGates, gate);
	}

	private static void RemoveByValue<T>(Dictionary<Transform, T> dictionary, GateBase gate) where T : GateBase
	{
		List<Transform> keys = new List<Transform>();
		foreach (KeyValuePair<Transform, T> pair in dictionary)
		{
			if (ReferenceEquals(pair.Value, gate))
			{
				keys.Add(pair.Key);
			}
		}
		for (int i = 0; i < keys.Count; i++)
		{
			dictionary.Remove(keys[i]);
		}
	}

	public static Multiplicator GetMultiplicator(Transform other)
	{
		Multiplicator result;
		return other != null && _multiplicators.TryGetValue(other, out result) ? result : null;
	}

	public static SubGate GetSubGate(Transform other)
	{
		SubGate result;
		return other != null && _subGates.TryGetValue(other, out result) ? result : null;
	}

	public static DivideGate GetDivideGate(Transform other)
	{
		DivideGate result;
		return other != null && _divideGates.TryGetValue(other, out result) ? result : null;
	}
}
