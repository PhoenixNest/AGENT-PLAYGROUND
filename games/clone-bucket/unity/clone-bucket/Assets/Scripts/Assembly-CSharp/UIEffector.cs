using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEffector
{
	private const float FullRotation = 90f;

	public static Sequence FadeInElement(GameObject element, float duration, Ease ease = Ease.OutQuad)
	{
		CanvasGroup group = GetOrAddCanvasGroup(element);
		element.SetActive(true);
		group.alpha = 0f;
		return DOTween.Sequence().Append(FadeGroup(group, 1f, duration, ease));
	}

	public static Tweener FadeInImage(Image image, float duration, Ease ease = Ease.OutQuad)
	{
		return FadeGraphic(image, image.color.a, duration, ease, true);
	}

	public static Tweener FadeInText(TextMeshProUGUI text, float duration, Ease ease = Ease.OutQuad)
	{
		return FadeGraphic(text, text.color.a, duration, ease, true);
	}

	public static Sequence FadeOutElement(GameObject element, float duration, Ease ease = Ease.OutQuad)
	{
		Sequence sequence = DOTween.Sequence();
		foreach (Image image in element.GetComponentsInChildren<Image>())
		{
			sequence.Insert(0f, FadeOutImage(image, duration, ease));
		}
		foreach (TextMeshProUGUI text in element.GetComponentsInChildren<TextMeshProUGUI>())
		{
			sequence.Insert(0f, FadeOutText(text, duration, ease));
		}
		Image ownImage = element.GetComponent<Image>();
		if (ownImage != null)
		{
			sequence.Insert(0f, FadeOutImage(ownImage, duration, ease));
		}
		TextMeshProUGUI ownText = element.GetComponent<TextMeshProUGUI>();
		if (ownText != null)
		{
			sequence.Insert(0f, FadeOutText(ownText, duration, ease));
		}
		return sequence;
	}

	public static Tweener FadeOutImage(Image image, float duration, Ease ease = Ease.OutQuad)
	{
		return FadeGraphic(image, 0f, duration, ease, false);
	}

	public static Tweener FadeOutText(TextMeshProUGUI text, float duration, Ease ease = Ease.OutQuad)
	{
		return FadeGraphic(text, 0f, duration, ease, false);
	}

	public static Tweener ZoomInElement(GameObject element, float duration, Ease ease = Ease.OutBack)
	{
		return ScaleTween(element, Vector3.zero, Vector3.one, duration, ease);
	}

	public static Tweener ZoomOutElement(GameObject element, float duration, Ease ease = Ease.OutBack)
	{
		return ScaleTween(element, Vector3.one, Vector3.zero, duration, ease);
	}

	public static Tweener ZoomInYElement(GameObject element, float duration, Ease ease = Ease.OutBack)
	{
		return ScaleTween(element, new Vector3(1f, 0f, 1f), Vector3.one, duration, ease);
	}

	public static Tweener ZoomOutYElement(GameObject element, float duration, Ease ease = Ease.OutBack)
	{
		return ScaleTween(element, Vector3.one, new Vector3(1f, 0f, 1f), duration, ease);
	}

	public static Tweener ZoomInXElement(GameObject element, float duration, Ease ease = Ease.OutBack)
	{
		return ScaleTween(element, new Vector3(0f, 1f, 1f), Vector3.one, duration, ease);
	}

	public static Tweener ZoomOutXElement(GameObject element, float duration, Ease ease = Ease.OutBack)
	{
		return ScaleTween(element, Vector3.one, new Vector3(0f, 1f, 1f), duration, ease);
	}

	public static Tweener SlideInUpElement(GameObject element, float offset, float duration, Ease ease = Ease.OutBack)
	{
		return Slide(element, Vector3.down, offset, duration, ease, true);
	}

	public static Tweener SlideInDownElement(GameObject element, float offset, float duration, Ease ease = Ease.OutBack)
	{
		return Slide(element, Vector3.up, offset, duration, ease, true);
	}

	public static Tweener SlideInRightElement(GameObject element, float offset, float duration, Ease ease = Ease.OutBack)
	{
		return Slide(element, Vector3.right, -offset, duration, ease, true);
	}

	public static Tweener SlideInLeftElement(GameObject element, float offset, float duration, Ease ease = Ease.OutBack)
	{
		return Slide(element, Vector3.left, offset, duration, ease, true);
	}

	public static Tweener SlideOutUpElement(GameObject element, float offset, float duration, Ease ease = Ease.OutBack)
	{
		return Slide(element, Vector3.up, offset, duration, ease, false);
	}

	public static Tweener SlideOutDownElement(GameObject element, float offset, float duration, Ease ease = Ease.OutBack)
	{
		return Slide(element, Vector3.down, offset, duration, ease, false);
	}

	public static Tweener SlideOutRightElement(GameObject element, float offset, float duration, Ease ease = Ease.OutBack)
	{
		return Slide(element, Vector3.right, offset, duration, ease, false);
	}

	public static Tweener SlideOutLeftElement(GameObject element, float offset, float duration, Ease ease = Ease.OutBack)
	{
		return Slide(element, Vector3.left, offset, duration, ease, false);
	}

	public static Tweener RotateInRightElement(GameObject element, float duration, Ease ease = Ease.OutQuad)
	{
		return Rotate(element, -FullRotation, 0f, duration, ease);
	}

	public static Tweener RotateInLeftElement(GameObject element, float duration, Ease ease = Ease.OutQuad)
	{
		return Rotate(element, FullRotation, 0f, duration, ease);
	}

	public static Tweener RotateOutRightElement(GameObject element, float duration, Ease ease = Ease.OutQuad)
	{
		return Rotate(element, 0f, -FullRotation, duration, ease);
	}

	public static Tweener RotateOutLeftElement(GameObject element, float duration, Ease ease = Ease.OutQuad)
	{
		return Rotate(element, 0f, FullRotation, duration, ease);
	}

	private static CanvasGroup GetOrAddCanvasGroup(GameObject element)
	{
		CanvasGroup group = element.GetComponent<CanvasGroup>();
		if (group == null)
		{
			group = element.AddComponent<CanvasGroup>();
		}
		return group;
	}

	private static Tweener FadeGroup(CanvasGroup group, float endAlpha, float duration, Ease ease)
	{
		Tweener tween = DOTween.To(() => group.alpha, value => group.alpha = value, endAlpha, duration);
		tween.SetEase(ease);
		return tween;
	}

	private static Tweener FadeGraphic(Graphic graphic, float endAlpha, float duration, Ease ease, bool fadeIn)
	{
		if (fadeIn)
		{
			Color start = graphic.color;
			start.a = 0f;
			graphic.color = start;
		}
		Tweener tween = DOTween.ToAlpha(() => graphic.color, value => graphic.color = value, endAlpha, duration);
		tween.SetEase(ease);
		return tween;
	}

	private static Tweener ScaleTween(GameObject element, Vector3 from, Vector3 to, float duration, Ease ease)
	{
		Transform target = element.transform;
		target.localScale = from;
		Tweener tween = target.DOScale(to, duration);
		tween.SetEase(ease);
		return tween;
	}

	private static Tweener Slide(GameObject element, Vector3 direction, float offset, float duration, Ease ease, bool incoming)
	{
		Transform target = element.transform;
		Vector3 rest = target.localPosition;
		Vector3 away = rest + direction * offset;
		Tweener tween;
		if (incoming)
		{
			target.localPosition = away;
			tween = target.DOLocalMove(rest, duration);
		}
		else
		{
			tween = target.DOLocalMove(away, duration);
		}
		tween.SetEase(ease);
		return tween;
	}

	private static Tweener Rotate(GameObject element, float fromZ, float toZ, float duration, Ease ease)
	{
		Transform target = element.transform;
		target.localEulerAngles = new Vector3(0f, 0f, fromZ);
		Tweener tween = target.DOLocalRotate(new Vector3(0f, 0f, toZ), duration, RotateMode.FastBeyond360);
		tween.SetEase(ease);
		return tween;
	}
}
