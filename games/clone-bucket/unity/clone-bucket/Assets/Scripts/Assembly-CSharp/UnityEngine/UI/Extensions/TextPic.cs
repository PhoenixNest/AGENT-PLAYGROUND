using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/TextPic")]
	[ExecuteInEditMode]
	public class TextPic : Text, IPointerClickHandler, IEventSystemHandler, IPointerExitHandler, IPointerEnterHandler, ISelectHandler
	{
		[Serializable]
		public struct IconName
		{
			public string name;

			public Sprite sprite;

			public Vector2 offset;

			public Vector2 scale;
		}

		[Serializable]
		public class HrefClickEvent : UnityEvent<string>
		{
		}

		[Serializable]
		public class HrefInfo
		{
			public int startIndex;

			public int endIndex;

			public string name;

			public readonly List<Rect> boxes;
		}

		public IconName[] inspectorIconList;

		[Tooltip("Global scaling factor for all images")]
		public float ImageScalingFactor;

		public string hyperlinkColor;

		public Vector2 imageOffset;

		public bool isCreating_m_HrefInfos;

		[SerializeField]
		private HrefClickEvent m_OnHrefClick;

		private readonly List<Image> m_ImagesPool;

		private readonly List<GameObject> culled_ImagesPool;

		private bool clearImages;

		private Object thisLock;

		private readonly List<int> m_ImagesVertexIndex;

		private static readonly Regex s_Regex;

		private static readonly Regex s_HrefRegex;

		private string fixedString;

		private bool updateQuad;

		private string m_OutputText;

		private Button button;

		private bool selected;

		private List<Vector2> positions;

		private string previousText;

		private readonly List<HrefInfo> m_HrefInfos;

		private static readonly StringBuilder s_TextBuilder;

		private MatchCollection matches;

		private MatchCollection href_matches;

		private MatchCollection removeCharacters;

		private int picIndex;

		private int vertIndex;

		private bool usesNewRendering;

		private static readonly Regex remove_Regex;

		private List<int> indexes;

		private int charactersRemoved;

		private int startCharactersRemoved;

		private int endCharactersRemoved;

		private int count;

		private int indexText;

		private string originalText;

		private UIVertex vert;

		private Vector2 lp;

		public HrefClickEvent onHrefClick
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public bool Selected
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public void ResetIconList()
		{
		}

		protected void UpdateQuadImage()
		{
		}

		private void Reset_m_HrefInfos()
		{
		}

		protected string GetOutputText()
		{
			return null;
		}

		public virtual void OnHrefClick(string hrefName)
		{
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
		}

		public void OnPointerClick(PointerEventData eventData)
		{
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		public void OnPointerExit(PointerEventData eventData)
		{
		}

		public void OnSelect(BaseEventData eventData)
		{
		}

		public void OnDeselect(BaseEventData eventData)
		{
		}

		public override void SetVerticesDirty()
		{
		}

		protected override void OnEnable()
		{
		}

		protected override void OnDisable()
		{
		}

		private new void Start()
		{
		}

		private void LateUpdate()
		{
		}
	}
}
