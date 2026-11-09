using UnityEngine;

public class TTPRateUsNotConnectedCanvas : MonoBehaviour
{
	public RectTransform mainPanelRectTransform;

	private void Start()
	{
	}

	public void OnClickClose()
	{
		gameObject.SetActive(false);
	}
}
