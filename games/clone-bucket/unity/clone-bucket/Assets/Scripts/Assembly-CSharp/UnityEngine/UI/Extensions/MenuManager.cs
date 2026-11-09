using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Menu Manager")]
	[DisallowMultipleComponent]
	public class MenuManager : MonoBehaviour
	{
		[SerializeField]
		private Menu[] menuScreens;

		[SerializeField]
		private int startScreen;

		private Stack<Menu> menuStack;

		public Menu[] MenuScreens
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public int StartScreen
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public static MenuManager Instance { get; set; }

		private void Start()
		{
		}

		private void OnDestroy()
		{
		}

		public GameObject CreateInstance(string MenuName)
		{
			return null;
		}

		public void CreateInstance(string MenuName, out GameObject menuInstance)
		{
			menuInstance = null;
		}

		public void OpenMenu(Menu menuInstance)
		{
		}

		private GameObject GetPrefab(string PrefabName)
		{
			return null;
		}

		public void CloseMenu(Menu menu)
		{
		}

		public void CloseTopMenu()
		{
		}

		private void Update()
		{
		}
	}
}
