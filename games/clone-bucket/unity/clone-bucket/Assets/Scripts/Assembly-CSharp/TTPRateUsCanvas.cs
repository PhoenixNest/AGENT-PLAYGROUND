using System;
using UnityEngine;

public class TTPRateUsCanvas : MonoBehaviour
{
	public RectTransform mainPanelRectTransform;

	private Action _goToStoreAction;

	private Action _neverAction;

	private Action _laterAction;

	private Action _closeAction;

	private void Start()
	{
	}

	public void Init(Action goToStoreAction, Action neverAction, Action laterAction, Action closeAction, string iconExtension)
	{
		_goToStoreAction = goToStoreAction;
		_neverAction = neverAction;
		_laterAction = laterAction;
		_closeAction = closeAction;
	}

	public void OnClickGoToStore()
	{
		_goToStoreAction?.Invoke();
		gameObject.SetActive(false);
	}

	public void OnClickNever()
	{
		_neverAction?.Invoke();
		gameObject.SetActive(false);
	}

	public void OnClickLater()
	{
		_laterAction?.Invoke();
		gameObject.SetActive(false);
	}

	public void OnClickClose()
	{
		_closeAction?.Invoke();
		gameObject.SetActive(false);
	}
}
