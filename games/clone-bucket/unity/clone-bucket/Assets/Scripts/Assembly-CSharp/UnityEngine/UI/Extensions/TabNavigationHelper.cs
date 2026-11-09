using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(EventSystem))]
	[AddComponentMenu("Event/Extensions/Tab Navigation Helper")]
	public class TabNavigationHelper : MonoBehaviour
	{
		private EventSystem _system;

		private Selectable StartingObject;

		private Selectable LastObject;

		[Tooltip("The path to take when user is tabbing through ui components.")]
		public Selectable[] NavigationPath;

		[Tooltip("Use the default Unity navigation system or a manual fixed order using Navigation Path")]
		public NavigationMode NavigationMode;

		[Tooltip("If True, this will loop the tab order from last to first automatically")]
		public bool CircularNavigation;

		private void Start()
		{
		}

		public void Update()
		{
		}

		private void SelectDefaultObject(out Selectable next)
		{
			next = null;
		}

		private void selectGameObject(Selectable selectable)
		{
		}
	}
}
