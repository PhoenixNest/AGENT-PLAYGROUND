using UnityEngine;

public class UIAdaptation : MonoBehaviour
{
	[SerializeField]
	private RectTransform settingsRect;

	[SerializeField]
	private RectTransform shopRect;

	private RectTransform selfRect;

	private Rect _lastSafeArea;

	private void Awake()
	{
		selfRect = GetComponent<RectTransform>();
		_lastSafeArea = new Rect(0f, 0f, -1f, -1f);
	}

	private void Update()
	{
		Rect safe = Screen.safeArea;
		if (safe == _lastSafeArea)
		{
			return;
		}
		_lastSafeArea = safe;

		float left = safe.xMin;
		float right = Screen.width - safe.xMax;
		float bottom = safe.yMin;
		float top = Screen.height - safe.yMax;
		ApplyInsets(settingsRect, left, right, bottom, top);
		ApplyInsets(shopRect, left, right, bottom, top);
	}

	private static void ApplyInsets(RectTransform rect, float left, float right, float bottom, float top)
	{
		if (rect == null)
		{
			return;
		}
		rect.offsetMin = new Vector2(left, bottom);
		rect.offsetMax = new Vector2(-right, -top);
	}
}
