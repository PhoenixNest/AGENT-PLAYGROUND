using System.Collections.Generic;
using UnityEngine;

public class ColorSchemeItemView : MonoBehaviour
{
	[SerializeField]
	private ColorSchemeItemType _itemType;

	[SerializeField]
	private MeshRenderer _meshRendererCached;

	[SerializeField]
	private bool _hideOnMaterialMissing;

	[SerializeField]
	private List<int> _customMaterialSlotsIndexes;

	private Material[] _slots;

	private MeshRenderer _slotsOwner;

	public ColorSchemeItemType ItemType => _itemType;

	public bool IsColorSchemeApplied { get; private set; }

	private void Reset()
	{
		_meshRendererCached = GetComponent<MeshRenderer>();
	}

	public void Show(Material material)
	{
		MeshRenderer meshRenderer = GetRenderer();
		if (meshRenderer == null)
		{
			return;
		}

		if (material == null)
		{
			IsColorSchemeApplied = false;
			if (_hideOnMaterialMissing)
			{
				meshRenderer.enabled = false;
			}

			return;
		}

		meshRenderer.enabled = true;

		if (_slots == null || _slotsOwner != meshRenderer)
		{
			_slots = meshRenderer.sharedMaterials;
			_slotsOwner = meshRenderer;
		}

		if (_slots.Length == 0)
		{
			return;
		}

		List<int> indexes = _customMaterialSlotsIndexes;
		if (indexes == null || indexes.Count == 0)
		{
			for (int i = 0; i < _slots.Length; i++)
			{
				_slots[i] = material;
			}
		}
		else
		{
			for (int i = 0; i < indexes.Count; i++)
			{
				int slot = indexes[i];
				if (slot >= 0 && slot < _slots.Length)
				{
					_slots[slot] = material;
				}
			}
		}

		meshRenderer.sharedMaterials = _slots;
		IsColorSchemeApplied = true;
	}

	public void CustomSetup(ColorSchemeItemType type, List<int> indexes)
	{
		_itemType = type;
		_customMaterialSlotsIndexes = indexes;
		_slots = null;
		_slotsOwner = null;
	}

	private MeshRenderer GetRenderer()
	{
		if (_meshRendererCached == null)
		{
			_meshRendererCached = GetComponent<MeshRenderer>();
		}

		return _meshRendererCached;
	}
}
