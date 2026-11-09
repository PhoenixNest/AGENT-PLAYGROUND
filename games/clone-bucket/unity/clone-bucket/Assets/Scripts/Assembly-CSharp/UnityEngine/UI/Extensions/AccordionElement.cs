using System;
using UnityEngine.UI.Extensions.Tweens;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform), typeof(LayoutElement))]
	[AddComponentMenu("UI/Extensions/Accordion/Accordion Element")]
	public class AccordionElement : Toggle
	{
		[SerializeField]
		private float m_MinHeight;

		[SerializeField]
		private float m_MinWidth;

		private Accordion m_Accordion;

		private RectTransform m_RectTransform;

		private LayoutElement m_LayoutElement;

		[NonSerialized]
		private readonly TweenRunner<FloatTween> m_FloatTweenRunner;

		public float MinHeight => 0f;

		public float MinWidth => 0f;

		protected AccordionElement()
		{
		}

		protected override void Awake()
		{
		}

		public void OnValueChanged(bool state)
		{
		}

		protected float GetExpandedHeight()
		{
			return 0f;
		}

		protected float GetExpandedWidth()
		{
			return 0f;
		}

		protected void StartTween(float startFloat, float targetFloat)
		{
		}

		protected void SetHeight(float height)
		{
		}

		protected void SetWidth(float width)
		{
		}
	}
}
