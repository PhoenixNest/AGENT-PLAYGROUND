using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/DragCorrector")]
	[RequireComponent(typeof(EventSystem))]
	public class DragCorrector : MonoBehaviour
	{
		public int baseTH;

		public int basePPI;

		public int dragTH;

		private void Start()
		{
		}
	}
}
