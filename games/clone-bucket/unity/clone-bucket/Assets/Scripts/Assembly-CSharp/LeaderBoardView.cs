using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using Levels;
using UnityEngine;

public class LeaderBoardView : MonoBehaviour
{
	internal const int RankTop = 15000;

	internal const int ScorePerRank = 30;

	internal const int MaxScore = 99999999;

	private const float Rank2ContentY = 286f;

	private const float Rank3ContentY = 143f;

	private const float Rank1PlayerY = 424f;

	private const float DefaultItemHeight = 100f;

	private const float ClimbStartDelay = 1.5f;

	private const float ClimbStepSeconds = 0.03f;

	private const float ClimbBaseSeconds = 0.5f;

	private const float RowMoveSeconds = 0.3f;

	private const float RowDropSeconds = 0.5f;

	public Transform Content1;

	public Transform Content2;

	public LeaderBoardItem ItemPrefab;

	public LeaderBoardItem playerItemPrefab;

	public int itemHeight;

	private List<LeaderBoardItem> items1 = new List<LeaderBoardItem>();

	private List<LeaderBoardItem> items2 = new List<LeaderBoardItem>();

	private List<LeaderBoardItem> items3 = new List<LeaderBoardItem>();

	private LeaderBoardItem playerItem;

	private Sequence sequence;

	private float ItemHeight => itemHeight > 0 ? itemHeight : DefaultItemHeight;

	private void OnEnable()
	{
		GameManager game = GameManager.instance;
		int levelScore = LevelsController.TotalFillCount;
		int lastScore = game != null ? game.LastLeaderBoardScore : 0;
		int currentScore = game != null ? game.LeaderBoardScore : 0;
		int position = Mathf.Max(1, game != null ? game.LeaderBoardPosition : 1);
		Create(levelScore, lastScore, position, currentScore);
	}

	private void OnDisable()
	{
		sequence?.Kill();
		sequence = null;
		ClearRows();
	}

	internal static int RankFromScore(int score)
	{
		return RankTop - (int)(score / (float)ScorePerRank);
	}

	internal static int ScoreFromRank(int rank)
	{
		long score = (RankTop - (long)rank) * ScorePerRank;
		if (score < 0)
		{
			score = 0;
		}
		if (score > MaxScore)
		{
			score = MaxScore;
		}
		return (int)score;
	}

	internal static int BelowCount(int position)
	{
		switch (position)
		{
			case 1:
				return 6;
			case 2:
				return 6;
			case 3:
				return 5;
			default:
				return 4;
		}
	}

	internal static int AboveCount(int targetRank)
	{
		uint steps = (uint)(targetRank - 1);
		return steps <= 2 ? (int)steps : 3;
	}

	internal static int ClimbSteps(int currentPosition, int currentScore)
	{
		int newRank = RankFromScore(currentScore);
		return currentPosition - (newRank > 0 ? newRank : 1);
	}

	internal static float ContentTargetY(int above, int climb, float itemHeight)
	{
		return itemHeight * ((3 - above) - climb);
	}

	internal static float ClimbDuration(int climb)
	{
		return climb * ClimbStepSeconds + ClimbBaseSeconds;
	}

	private static bool TryGetLandingY(int targetRank, out float y)
	{
		switch (targetRank)
		{
			case 3:
				y = Rank3ContentY;
				return true;
			case 2:
				y = Rank2ContentY;
				return true;
			case 1:
				y = Rank1PlayerY;
				return true;
			default:
				y = 0f;
				return false;
		}
	}

	private void Create(int levelScore, int lastScore, int currentPosition, int currentScore)
	{
		ClearRows();
		RectTransform content = Content1 as RectTransform;
		if (content == null || ItemPrefab == null)
		{
			return;
		}

		Random.State savedState = Random.state;
		try
		{
			int climb = ClimbSteps(currentPosition, currentScore);
			int targetRank = currentPosition - climb;

			LeaderBoardItem prefab = playerItemPrefab != null ? playerItemPrefab : ItemPrefab;
			playerItem = Instantiate(prefab, Content1);
			playerItem.transform.localPosition = Vector3.zero;
			playerItem.SetData(new LeaderBoardData(new Vector3(0f, 0f, 1f), currentPosition, "YOU", lastScore.ToString(CultureInfo.InvariantCulture), currentPosition, true));

			int rnd = Random.Range(1, 10000);

			if (currentPosition == 2)
			{
				SetAnchoredPosition(Content1, new Vector2(0f, Rank2ContentY));
			}
			else if (currentPosition == 3)
			{
				SetAnchoredPosition(Content1, new Vector2(0f, Rank3ContentY));
			}

			int below = BelowCount(currentPosition);
			for (int i = 0; i < below; i++)
			{
				int offset = rnd + i + currentPosition;
				LeaderBoardItem item = Instantiate(ItemPrefab, Content1);
				item.SetData(new LeaderBoardData(new Vector3(0f, -ItemHeight * (i + 1), 1f), currentPosition + i + 1, NickFlagManager.instance != null ? NickFlagManager.instance.GetNick(offset) : "Player", "", offset, false));
				items3.Add(item);
			}

			if (currentPosition == 1)
			{
				RequestNextButton();
				SetAnchoredPosition(Content1, Vector2.zero);
				return;
			}

			int above = AboveCount(targetRank);
			for (int j = 0; j < above + climb; j++)
			{
				if (j >= climb && targetRank < 2)
				{
					break;
				}
				int k = rnd + above + j;
				LeaderBoardItem item = Instantiate(ItemPrefab, Content1);
				item.SetData(new LeaderBoardData(new Vector3(0f, ItemHeight * (j + 1), 0f), currentPosition - 1 - j, NickFlagManager.instance != null ? NickFlagManager.instance.GetNick(k + currentPosition) : "Player", "", k + currentPosition, false));
				if (j < climb)
				{
					items1.Add(item);
				}
				else
				{
					items2.Add(item);
				}
			}

			float stopY = ContentTargetY(above, climb, ItemHeight);
			sequence = DOTween.Sequence()
				.AppendInterval(ClimbStartDelay)
				.AppendCallback(() => OnClimbStart(targetRank))
				.Append(content.DOAnchorPosY(stopY, ClimbDuration(climb)))
				.Join(DOTween.To(() => currentPosition, x => SetPlayerRank(x), targetRank, ClimbDuration(climb)))
				.Join(DOTween.To(() => lastScore, x => SetPlayerScore(x), lastScore + levelScore, ClimbDuration(climb)))
				.AppendCallback(() => OnPassedRowsDrop(targetRank))
				.AppendCallback(() => OnClimbEnd(targetRank));
		}
		finally
		{
			Random.state = savedState;
		}
	}

	private void SetPlayerRank(int rank)
	{
		if (playerItem != null && playerItem.PositionText != null)
		{
			playerItem.PositionText.text = "#" + rank;
		}
	}

	private void SetPlayerScore(int score)
	{
		if (playerItem != null && playerItem.Score != null)
		{
			playerItem.Score.text = score.ToString(CultureInfo.InvariantCulture);
		}
	}

	private void OnClimbStart(int targetRank)
	{
		foreach (LeaderBoardItem item in items3)
		{
			if (item != null)
			{
				item.transform.DOLocalMoveY(item.transform.localPosition.y + ItemHeight, RowMoveSeconds);
			}
		}
		if (playerItem == null)
		{
			return;
		}
		Transform grandParent = Content1 != null && Content1.parent != null ? Content1.parent.parent : null;
		if (grandParent != null)
		{
			playerItem.transform.SetParent(grandParent);
		}
		if (TryGetLandingY(targetRank, out float y) && playerItem.rectTransform != null)
		{
			playerItem.rectTransform.DOAnchorPosY(y, RowMoveSeconds);
		}
	}

	private void OnPassedRowsDrop(int targetRank)
	{
		foreach (LeaderBoardItem item in items1)
		{
			if (item == null)
			{
				continue;
			}
			if (item.PositionText != null && int.TryParse(item.PositionText.text.Replace("#", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out int rank))
			{
				item.PositionText.text = "#" + (rank + 1);
			}
			float y = item.transform.localPosition.y;
			if (item.rectTransform != null)
			{
				item.rectTransform.DOAnchorPosY(y - ItemHeight, RowDropSeconds);
			}
			if (targetRank < 4)
			{
				item.UpdateData(-1);
			}
		}
		foreach (LeaderBoardItem item in items3)
		{
			if (item != null && item.rectTransform != null)
			{
				float y = item.transform.localPosition.y;
				item.rectTransform.DOAnchorPosY(y - ItemHeight, RowDropSeconds);
			}
		}
	}

	private void OnClimbEnd(int targetRank)
	{
		if (playerItem != null)
		{
			playerItem.transform.SetParent(Content1);
			if (targetRank < 4)
			{
				playerItem.UpdateData(targetRank);
			}
		}
		RequestNextButton();
	}

	private static void RequestNextButton()
	{
		CompleteLevelScreen screen = GameManager.instance != null ? GameManager.instance.completeLevelScreen : null;
		if (screen != null)
		{
			screen.ShowNextBtn();
		}
	}

	private void ClearRows()
	{
		sequence?.Kill();
		sequence = null;
		if (playerItem != null)
		{
			Destroy(playerItem.gameObject);
		}
		playerItem = null;
		DestroyChildren(Content1);
		DestroyChildren(Content2);
		items1 = new List<LeaderBoardItem>();
		items2 = new List<LeaderBoardItem>();
		items3 = new List<LeaderBoardItem>();
		SetAnchoredPosition(Content1, Vector2.zero);
		SetAnchoredPosition(Content2, Vector2.zero);
	}

	private static void DestroyChildren(Transform parent)
	{
		if (parent == null)
		{
			return;
		}
		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			Destroy(parent.GetChild(i).gameObject);
		}
	}

	private static void SetAnchoredPosition(Transform target, Vector2 position)
	{
		if (target is RectTransform rect)
		{
			rect.anchoredPosition = position;
		}
	}
}
