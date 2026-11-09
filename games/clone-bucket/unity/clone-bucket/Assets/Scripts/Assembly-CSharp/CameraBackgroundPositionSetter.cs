using System;
using UnityEngine;

public class CameraBackgroundPositionSetter : MonoBehaviour
{
	[SerializeField]
	private Transform[] _backgrounds;

	[NonSerialized]
	private float[] _backgroundZPositions;

	private void Start()
	{
		if (_backgrounds == null)
		{
			_backgroundZPositions = new float[0];
			return;
		}
		_backgroundZPositions = new float[_backgrounds.Length];
		for (int i = 0; i < _backgrounds.Length; i++)
		{
			_backgroundZPositions[i] = _backgrounds[i] != null ? _backgrounds[i].position.z : 0f;
		}
	}

	private void LateUpdate()
	{
		if (_backgrounds == null || _backgroundZPositions == null)
		{
			return;
		}
		int count = Mathf.Min(_backgrounds.Length, _backgroundZPositions.Length);
		for (int i = 0; i < count; i++)
		{
			Transform background = _backgrounds[i];
			if (background == null)
			{
				continue;
			}
			Vector3 p = background.position;
			p.z = _backgroundZPositions[i];
			background.position = p;
		}
	}
}
