using System.Collections.Generic;
using Game.Levels;

namespace Levels
{
	public struct MiniLevelData
	{
		public int miniLevelIndex;

		public int missionIndex;

		public string levelName;

		public int startBallAmount;

		public int targetBallAmount;

		public int perfectBallAmount;

		public bool isBossLevel;

		public LevelType levelType;

		public List<GateType> gateTypes;

		public List<int> gateValues;

		public string mazeMaterialName;

		public override string ToString()
		{
			return levelName;
		}
	}
}
