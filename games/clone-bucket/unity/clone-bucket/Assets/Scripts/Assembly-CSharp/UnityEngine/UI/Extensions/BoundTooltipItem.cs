namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Bound Tooltip/Bound Tooltip Item")]
	public class BoundTooltipItem : MonoBehaviour
	{
		public Text TooltipText;

		public Vector3 ToolTipOffset;

		private static BoundTooltipItem instance;

		public bool IsActive => false;

		public static BoundTooltipItem Instance => null;

		private void Awake()
		{
		}

		public void ShowTooltip(string text, Vector3 pos)
		{
		}

		public void HideTooltip()
		{
		}
	}
}
