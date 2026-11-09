using UnityEngine;
using Zenject;

namespace Levels
{
	public class LevelsControllerInstaller : MonoInstaller
	{
		[SerializeField]
		private LevelsController _controller;

		public override void InstallBindings()
		{
			if (_controller != null)
			{
				Container.BindInstance(_controller).AsSingle();
			}
		}
	}
}
