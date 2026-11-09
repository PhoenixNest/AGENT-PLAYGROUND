using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardItem : MonoBehaviour
{
	private const int RowSeed = 3141;

	private const int FlagImageCount = 34;

	private const int IconImageCount = 4;

	private const int PlayerIconIndex = 4;

	private const int MedalCount = 3;

	private const string NormalBackground = "normal";

	private const string SelfBackground = "self";

	private static readonly Color PlayerNameColor = new Color(0.121569f, 0.309804f, 0.090196f, 1f);

	private static readonly Vector2 PlayerRowSize = new Vector2(-10f, 152f);

	public int Position;

	public TextMeshProUGUI PositionText;

	public TextMeshProUGUI Nickname;

	public TextMeshProUGUI Score;

	public Image Flag;

	public RectTransform rectTransform;

	public CustomImage bgCustomImage;

	public CustomImage iconCustomImage;

	public CustomImage flagCustomImage;

	public CustomImage posCustomImage;

	private LeaderBoardData mydata;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	public void SetData(LeaderBoardData data)
	{
		if (data == null)
		{
			return;
		}
		mydata = data;
		if (data.isPlayer)
		{
			SetUpPlayer(data);
		}
		else
		{
			SetUpRow(data);
		}
		ApplyMedal(data.posindex);
		Position = data.posindex;
		if (rectTransform != null)
		{
			rectTransform.anchoredPosition3D = data.position;
		}
		if (PositionText != null)
		{
			PositionText.text = "#" + data.posindex;
		}
		if (Nickname != null)
		{
			Nickname.text = data.nickname;
		}
	}

	public void UpdateData(int posindex = -1)
	{
		if (posindex == -1)
		{
			posindex = (mydata != null ? mydata.posindex : Position) + 1;
		}
		Position = posindex;
		if (mydata != null)
		{
			mydata.posindex = posindex;
		}
		ApplyMedal(posindex);
	}

	private void SetUpRow(LeaderBoardData data)
	{
		Random.State savedState = Random.state;
		Random.InitState(data.offset + RowSeed);
		int flagIndex = Random.Range(0, FlagImageCount);
		int iconIndex = Random.Range(0, IconImageCount);
		Random.state = savedState;

		if (flagCustomImage != null)
		{
			flagCustomImage.SetImageByIndex(flagIndex);
		}
		if (iconCustomImage != null)
		{
			iconCustomImage.SetImageByIndex(iconIndex);
		}
		if (Flag != null)
		{
			Flag.color = Color.white;
		}
		if (Score != null)
		{
			Score.text = LeaderBoardView.ScoreFromRank(data.posindex).ToString();
		}
		if (bgCustomImage != null)
		{
			bgCustomImage.SetImageByName(NormalBackground);
		}
	}

	private void SetUpPlayer(LeaderBoardData data)
	{
		if (Score != null)
		{
			Score.gameObject.SetActive(true);
		}
		Image background = GetComponent<Image>();
		if (background != null)
		{
			background.color = Color.white;
		}
		if (Flag != null)
		{
			Flag.color = new Color(1f, 1f, 1f, 0f);
		}
		if (Nickname != null)
		{
			Nickname.color = PlayerNameColor;
		}
		if (iconCustomImage != null)
		{
			iconCustomImage.SetImageByIndex(PlayerIconIndex);
		}
		if (bgCustomImage != null)
		{
			bgCustomImage.SetImageByName(SelfBackground);
		}
		if (rectTransform != null)
		{
			rectTransform.sizeDelta = PlayerRowSize;
		}
		if (Score != null)
		{
			Score.text = data.score ?? "";
		}
	}

	private void ApplyMedal(int posindex)
	{
		bool isMedal = posindex >= 1 && posindex <= MedalCount;
		if (posCustomImage != null)
		{
			Image medal = posCustomImage.GetComponent<Image>();
			if (medal != null)
			{
				medal.enabled = isMedal;
			}
			if (isMedal)
			{
				posCustomImage.SetImageByIndex(posindex - 1);
			}
		}
		if (PositionText != null)
		{
			PositionText.gameObject.SetActive(!isMedal);
		}
	}
}
