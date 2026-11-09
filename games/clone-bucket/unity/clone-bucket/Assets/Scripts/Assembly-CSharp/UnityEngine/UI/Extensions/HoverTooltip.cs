namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/HoverTooltip")]
	public class HoverTooltip : MonoBehaviour
	{
		public int horizontalPadding;

		public int verticalPadding;

		public Text thisText;

		public HorizontalLayoutGroup hlG;

		public RectTransform bgImage;

		private Image bgImageSource;

		private bool firstUpdate;

		private bool inside;

		private RenderMode GUIMode;

		private Camera GUICamera;

		private Vector3 lowerLeft;

		private Vector3 upperRight;

		private float currentYScaleFactor;

		private float currentXScaleFactor;

		private float defaultYOffset;

		private float defaultXOffset;

		private float tooltipRealHeight;

		private float tooltipRealWidth;

		private void Start()
		{
		}

		public void SetTooltip(string text)
		{
		}

		public void SetTooltip(string[] texts)
		{
		}

		public void SetTooltip(string text, bool test)
		{
		}

		public void OnScreenSpaceCamera()
		{
		}

		public void HideTooltip()
		{
		}

		private void Update()
		{
		}

		private void LayoutInit()
		{
		}

		private void NewTooltip()
		{
		}

		public void ActivateTooltipVisibility()
		{
		}

		public void HideTooltipVisibility()
		{
		}
	}
}
