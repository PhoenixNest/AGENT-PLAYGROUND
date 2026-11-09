using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
	public class LevelView : MonoBehaviour
	{
		public Transform transformCached;

		[HideInInspector]
		public List<MiniLevelView> miniLevelViews;

		public void Clear()
		{
			if (miniLevelViews == null)
			{
				return;
			}

			foreach (MiniLevelView view in miniLevelViews)
			{
				if (view != null)
				{
					Destroy(view.gameObjectCached != null ? view.gameObjectCached : view.gameObject);
				}
			}

			miniLevelViews.Clear();
		}
	}
}
