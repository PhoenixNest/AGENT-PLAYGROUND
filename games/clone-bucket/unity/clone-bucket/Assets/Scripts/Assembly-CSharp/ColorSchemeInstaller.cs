using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "Installers/ColorSchemeInstaller", fileName = "ColorSchemeInstaller")]
public class ColorSchemeInstaller : ScriptableObjectInstaller
{
	public ColorSchemesConfig colorSchemesConfig;

	public override void InstallBindings()
	{
		if (colorSchemesConfig != null)
		{
			Container.BindInstance(colorSchemesConfig).AsSingle();
		}

		Container.Bind<IColorSchemeController>().To<ColorSchemeController>().AsSingle();
	}
}
