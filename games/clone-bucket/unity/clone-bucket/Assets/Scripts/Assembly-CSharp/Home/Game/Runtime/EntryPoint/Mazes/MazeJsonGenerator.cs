using System;
using UnityEngine;

namespace Home.Game.Runtime.EntryPoint.Mazes
{
	public class MazeJsonGenerator : MonoBehaviour
	{
		[SerializeField]
		private Maze _maze;

		[SerializeField]
		private MazePathGeneratorConfig _config;

		[NonSerialized]
		public GateBase[] gates;

		[NonSerialized]
		public MazeGateDefinition[] gateDefinitions;

		private void Reset()
		{
		}

		public void ApplyFromJson(string json)
		{
		}

		private Transform FindGatesContainer()
		{
			return null;
		}

		private void ResetMaze(Transform gatesContainer)
		{
		}

		private void ApplyWallStates(MazeDefinition definition)
		{
		}

		private void ApplyEndPosition(MazeDefinition definition)
		{
		}

		private void ApplyDynamicWalls(MazeDefinition definition)
		{
		}

		private void ApplyRotators(MazeDefinition definition)
		{
		}

		private void SpawnGates(MazeDefinition definition, Transform gatesContainer)
		{
		}

		private MazeGateTypeConfig FindGateTypeConfig(string typeName)
		{
			return null;
		}

		private static bool IsConnectionWalled(MazeCellConnection connection)
		{
			return false;
		}
	}
}
