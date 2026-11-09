using TMPro;
using UnityEngine;

public class CreditScoreBar : MonoBehaviour
{
	public RectTransform Arrow;

	public TextMeshProUGUI ValueText;

	private void OnEnable()
	{
		if (Arrow == null)
		{
			return;
		}
		Arrow.anchoredPosition = new Vector2(ArrowX(0), Arrow.anchoredPosition.y);
	}

	public void SetValue(int percent)
	{
		int clamped = Mathf.Clamp(percent, 0, 100);
		if (Arrow != null)
		{
			Arrow.anchoredPosition = new Vector2(ArrowX(clamped), Arrow.anchoredPosition.y);
		}
		if (ValueText != null)
		{
			ValueText.text = clamped + "%";
		}
	}

	internal static float ArrowXForPercent(int percent, float barWidth)
	{
		float t = Mathf.Clamp01(percent / 100f);
		return Mathf.Lerp(-barWidth * 0.5f, barWidth * 0.5f, t);
	}

	private float ArrowX(int percent)
	{
		float width = 0f;
		RectTransform parent = Arrow.parent as RectTransform;
		if (parent != null)
		{
			width = parent.rect.width;
		}
		return ArrowXForPercent(percent, width);
	}
}
