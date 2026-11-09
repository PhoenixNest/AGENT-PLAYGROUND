using UnityEngine;

[CreateAssetMenu(fileName = "GatesConfig", menuName = "Configs/GatesConfig")]
public class GatesConfig : ScriptableObject
{
	public SubGate substractionGatePrefab;

	public Multiplicator multiplicationGatePrefab;

	public DivideGate dividerGatePrefab;
}
