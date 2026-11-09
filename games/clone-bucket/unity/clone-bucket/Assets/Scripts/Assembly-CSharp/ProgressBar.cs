using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	public Image Fill;

	public Image FillLeft;

	public Image FillRight;

	public Image Background;

	public Image BackgroundLeft;

	public Image BackgroundRight;

	public Slider ProgressSlider;

	public Color FillColor;

	public Color BackgroundColor;

	internal static float ClampProgress(float progressValue)
	{
		if (float.IsNaN(progressValue))
		{
			return 0f;
		}
		return Mathf.Clamp01(progressValue);
	}

	private void OnDisable()
	{
		SetProgress(0f);
	}

	public void SetProgress(float progressValue)
	{
		float progress = ClampProgress(progressValue);

		if (ProgressSlider != null)
		{
			ProgressSlider.value = progress;
		}

		if (Fill != null)
		{
			Fill.fillAmount = progress;
			Fill.color = FillColor;
		}
		if (FillLeft != null)
		{
			FillLeft.enabled = progress > 0f;
			FillLeft.color = FillColor;
		}
		if (FillRight != null)
		{
			FillRight.enabled = progress >= 1f;
			FillRight.color = FillColor;
		}

		if (Background != null)
		{
			Background.color = BackgroundColor;
		}
		if (BackgroundLeft != null)
		{
			BackgroundLeft.color = BackgroundColor;
		}
		if (BackgroundRight != null)
		{
			BackgroundRight.color = BackgroundColor;
		}
	}
}
