using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(Selectable))]
	public class StepperSide : UIBehaviour, IPointerClickHandler, IEventSystemHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
	{
		internal Sprite cutSprite;

		private Selectable button => null;

		private Stepper stepper => null;

		private bool leftmost => false;

		protected StepperSide()
		{
		}

		public virtual void OnPointerClick(PointerEventData eventData)
		{
		}

		public virtual void OnSubmit(BaseEventData eventData)
		{
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
		}

		public virtual void OnPointerExit(PointerEventData eventData)
		{
		}

		public virtual void OnPointerDown(PointerEventData eventData)
		{
		}

		public virtual void OnPointerUp(PointerEventData eventData)
		{
		}

		public virtual void OnSelect(BaseEventData eventData)
		{
		}

		public virtual void OnDeselect(BaseEventData eventData)
		{
		}

		private void Press()
		{
		}

		private void AdjustSprite(bool restore)
		{
		}
	}
}
