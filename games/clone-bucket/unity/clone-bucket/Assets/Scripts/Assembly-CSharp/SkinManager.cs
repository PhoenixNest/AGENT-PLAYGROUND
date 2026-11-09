using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
	internal const string DefaultSkinName = "Ball1";

	internal const string SelectedSkinKey = "current_ball";

	internal const string ShopVariantKey = "DefaultBall";

	public static SkinManager instance;

	[SerializeField]
	private ShopConfig[] _configs;

	[System.NonSerialized]
	private ShopConfig _config;

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
			return;
		}

		instance = this;

		int variant = ParseVariant(RemoteConfigHelper.GetString(ShopVariantKey, "0"));
		_config = PickVariant(_configs, variant);

		if (!PlayerPrefs.HasKey(SelectedSkinKey))
		{
			InitializeSaveState();
		}
	}

	private void InitializeSaveState()
	{
		List<string> names = GetSkinNames();
		for (int i = 0; i < names.Count; i++)
		{
			PlayerPrefs.SetInt(names[i], 0);
		}

		if (names.Count > 0)
		{
			PlayerPrefs.SetInt(names[0], 1);
			PlayerPrefs.SetString(SelectedSkinKey, names[0]);
		}
	}

	internal static int ParseVariant(string value)
	{
		int index;
		return int.TryParse(value, out index) ? index : 0;
	}

	internal static ShopConfig PickVariant(ShopConfig[] configs, int index)
	{
		if (configs == null || configs.Length == 0)
		{
			return null;
		}

		if (index < 0 || index >= configs.Length)
		{
			index = 0;
		}

		return configs[index];
	}

	public SkinBallItem GetBall(string name)
	{
		return FindBall(name);
	}

	public SkinBallItem GetDefaultBall()
	{
		return _config != null && _config.balls != null && _config.balls.Count > 0 ? _config.balls[0] : null;
	}

	public SkinBallItem GetLockedItem()
	{
		if (_config == null || _config.balls == null)
		{
			return null;
		}

		for (int i = 0; i < _config.balls.Count; i++)
		{
			SkinBallItem item = _config.balls[i];
			if (item != null && !IsOwned(item.Name))
			{
				return item;
			}
		}

		return null;
	}

	public List<SkinBallItem> GetLockedItems()
	{
		var result = new List<SkinBallItem>();
		if (_config == null || _config.balls == null)
		{
			return result;
		}

		for (int i = 0; i < _config.balls.Count; i++)
		{
			SkinBallItem item = _config.balls[i];
			if (item != null && !IsOwned(item.Name))
			{
				result.Add(item);
			}
		}

		return result;
	}

	public bool IsOwned(string name)
	{
		return name != null && PlayerPrefs.GetInt(name, 0) == 1;
	}

	public bool MarkOwned(string name)
	{
		if (!IsOwned(name) && GetSkinNames().Contains(name))
		{
			PlayerPrefs.SetInt(name, 1);
			return true;
		}

		return false;
	}

	public List<string> GetOwnedNames()
	{
		var result = new List<string>();
		List<string> names = GetSkinNames();
		for (int i = 0; i < names.Count; i++)
		{
			if (IsOwned(names[i]))
			{
				result.Add(names[i]);
			}
		}

		return result;
	}

	public void SetOwnedNames(IEnumerable<string> names)
	{
		var owned = new HashSet<string>();
		if (names != null)
		{
			foreach (string name in names)
			{
				if (!string.IsNullOrEmpty(name))
				{
					owned.Add(name);
				}
			}
		}

		List<string> skinNames = GetSkinNames();
		for (int i = 0; i < skinNames.Count; i++)
		{
			PlayerPrefs.SetInt(skinNames[i], owned.Contains(skinNames[i]) ? 1 : 0);
		}
	}

	internal static bool IsOwnedName(ISet<string> owned, string name)
	{
		return name != null && owned.Contains(name);
	}

	internal static bool MarkOwnedName(ISet<string> owned, IList<string> skinNames, string name)
	{
		if (name == null || owned.Contains(name) || !skinNames.Contains(name))
		{
			return false;
		}

		owned.Add(name);
		return true;
	}

	internal static List<int> LockedIndices(IList<string> skinNames, ISet<string> owned)
	{
		var result = new List<int>();
		for (int i = 0; i < skinNames.Count; i++)
		{
			if (!IsOwnedName(owned, skinNames[i]))
			{
				result.Add(i);
			}
		}

		return result;
	}

	private SkinBallItem FindBall(string name)
	{
		if (_config == null || _config.balls == null || name == null)
		{
			return null;
		}

		for (int i = 0; i < _config.balls.Count; i++)
		{
			SkinBallItem item = _config.balls[i];
			if (item != null && item.Name == name)
			{
				return item;
			}
		}

		return null;
	}

	private List<string> GetSkinNames()
	{
		var names = new List<string>();
		if (_config == null || _config.balls == null)
		{
			return names;
		}

		for (int i = 0; i < _config.balls.Count; i++)
		{
			if (_config.balls[i] != null)
			{
				names.Add(_config.balls[i].Name);
			}
		}

		return names;
	}
}
