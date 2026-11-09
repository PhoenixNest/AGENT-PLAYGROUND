using System.Collections.Generic;
using UnityEngine;

public class NickFlagManager : SingletonBehaviour<NickFlagManager>
{
	private const string NamesResourcePath = "textfiles/names";

	private const int NameSeed = 3141;

	public List<Sprite> flags;

	private static List<string> m_namesList;

	private TextAsset m_textAsset;

	protected override void Awake()
	{
		base.Awake();
		ReadTextFile();
	}

	private void ReadTextFile()
	{
		m_namesList = new List<string>();
		if (m_textAsset == null)
		{
			m_textAsset = Resources.Load<TextAsset>(NamesResourcePath);
		}
		if (m_textAsset != null)
		{
			foreach (string line in m_textAsset.text.Split('\n'))
			{
				string name = line.Trim();
				if (name.Length > 0)
				{
					m_namesList.Add(name);
				}
			}
		}
		if (m_namesList.Count == 0)
		{
			m_namesList.Add("Player");
		}
	}

	public string GetNick(int offset)
	{
		if (m_namesList == null || m_namesList.Count == 0)
		{
			ReadTextFile();
		}
		Random.State savedState = Random.state;
		Random.InitState(offset + NameSeed);
		string name = m_namesList[Random.Range(0, m_namesList.Count)];
		Random.state = savedState;
		return name;
	}

	public Sprite GetFlag(int offset)
	{
		if (flags == null || flags.Count == 0)
		{
			return null;
		}
		Random.State savedState = Random.state;
		Random.InitState(offset + NameSeed);
		Sprite flag = flags[Random.Range(0, flags.Count)];
		Random.state = savedState;
		return flag;
	}
}
