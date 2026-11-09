using UnityEngine;

public class LeaderBoardData
{
	public Vector3 position;

	public int posindex;

	public string nickname;

	public string score;

	public int offset;

	public bool isPlayer;

	public LeaderBoardData(Vector3 pos, int posind, string name, string sc, int _offset, bool player = false)
	{
		position = pos;
		posindex = posind;
		nickname = name;
		score = sc;
		offset = _offset;
		isPlayer = player;
	}
}
