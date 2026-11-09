namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("UI/Effects/Extensions/UIImageCrop")]
	[ExecuteInEditMode]
	public class UIImageCrop : MonoBehaviour
	{
		private MaskableGraphic mGraphic;

		private Material mat;

		private int XCropProperty;

		private int YCropProperty;

		public float XCrop;

		public float YCrop;

		private void Start()
		{
		}

		public void SetMaterial()
		{
		}

		public void OnValidate()
		{
		}

		public void SetXCrop(float xcrop)
		{
		}

		public void SetYCrop(float ycrop)
		{
		}
	}
}
