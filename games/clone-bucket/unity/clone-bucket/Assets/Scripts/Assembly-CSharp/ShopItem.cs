using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
	private const float SelectedScale = 1.15f;

	private const float ScaleDuration = 0.2f;

	public string ItemName;

	public GameObject LockImg;

	public CustomImage Icon;

	public Animator UnlockAnim;

	public RectTransform rect;

	public Image ballImg;

	public Action<ShopItem> OnClicked;

	private void Awake()
	{
		if (rect == null)
		{
			rect = GetComponent<RectTransform>();
		}
	}

	private void OnDisable()
	{
		DOTween.Kill(this);
	}

	public void OnButtonClick()
	{
		if (OnClicked != null)
		{
			OnClicked(this);
		}
	}

	public void SetLocked(bool locked)
	{
		if (LockImg != null)
		{
			LockImg.SetActive(locked);
		}
	}

	public void SetSelected(bool selected, bool enlarged = false)
	{
		if (rect == null)
		{
			return;
		}

		float target = selected && enlarged ? SelectedScale : 1f;
		DOTween.Kill(this);
		rect.DOScale(Vector3.one * target, ScaleDuration).SetTarget(this);
	}

	public void PlayUnlockAnim()
	{
		if (UnlockAnim != null && UnlockAnim.isActiveAndEnabled)
		{
			UnlockAnim.Play(0, -1, 0f);
		}
		else
		{
			SetLocked(false);
		}
	}

	public void AnimatorEnd()
	{
		SetLocked(false);
	}
}
