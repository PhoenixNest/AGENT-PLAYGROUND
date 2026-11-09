namespace UnityEngine.UI.Extensions
{
	public class ExampleSelectable : MonoBehaviour, IBoxSelectable
	{
		private bool _selected;

		private bool _preSelected;

		private SpriteRenderer spriteRenderer;

		private Image image;

		private Text text;

		public bool selected
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool preSelected
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		Transform IBoxSelectable.transform => null;

		private void Start()
		{
		}

		private void Update()
		{
		}
	}
}
