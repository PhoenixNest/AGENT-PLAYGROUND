using UnityEngine;

namespace Home.Game.Runtime.EntryPoint.UI
{
	public class UIRotateSimple : MonoBehaviour
	{
		[SerializeField]
		private Transform _transformCached;

		[SerializeField]
		private float _rotateSpeed;

		private void Update()
		{
			if (_transformCached == null)
			{
				_transformCached = transform;
			}
			_transformCached.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime, Space.Self);
		}

		private void Reset()
		{
			_transformCached = transform;
		}
	}
}
