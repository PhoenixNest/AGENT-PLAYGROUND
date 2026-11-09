using UnityEngine;

public class SocialNotConnectedCanvas : MonoBehaviour
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
