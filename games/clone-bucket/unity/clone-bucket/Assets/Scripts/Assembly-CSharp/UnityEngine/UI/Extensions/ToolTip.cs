namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("UI/Extensions/Tooltip/Tooltip")]
	public class ToolTip : MonoBehaviour
	{
		private Text _text;

		private RectTransform _rectTransform;

		private RectTransform canvasRectTransform;

		[Tooltip("The canvas used by the tooltip as positioning and scaling reference. Should usually be the root Canvas of the hierarchy this component is in")]
		public Canvas canvas;

		[Tooltip("Sets if tooltip triggers will run ForceUpdateCanvases and refresh the tooltip's layout group (if any) when hovered, in order to prevent momentousness misplacement sometimes caused by ContentSizeFitters")]
		public bool tooltipTriggersCanForceCanvasUpdate;

		private LayoutGroup _layoutGroup;

		private bool _inside;

		private float width;

		private float height;

		public float YShift;

		public float xShift;

		[HideInInspector]
		public RenderMode guiMode;

		private Camera _guiCamera;

		private Vector3 screenLowerLeft;

		private Vector3 screenUpperRight;

		private Vector3 shiftingVector;

		private Vector3 baseTooltipPos;

		private Vector3 newTTPos;

		private Vector3 adjustedNewTTPos;

		private Vector3 adjustedTTLocalPos;

		private Vector3 shifterForBorders;

		private float borderTest;

		private static ToolTip instance;

		public Camera GuiCamera => null;

		public static ToolTip Instance => null;

		private void Reset()
		{
		}

		public void Awake()
		{
		}

		public void SetTooltip(string ttext)
		{
		}

		public void SetTooltip(string ttext, Vector3 basePos, bool refreshCanvasesBeforeGetSize = false)
		{
		}

		public void HideTooltip()
		{
		}

		private void Update()
		{
		}

		public void RefreshTooltipSize()
		{
		}

		public void ContextualTooltipUpdate(bool refreshCanvasesBeforeGettingSize = false)
		{
		}

		public void OnScreenSpaceCamera(bool refreshCanvasesBeforeGettingSize = false)
		{
		}

		public void OnScreenSpaceOverlay(bool refreshCanvasesBeforeGettingSize = false)
		{
		}
	}
}
