using NUnit.Framework;

public class GameAssemblySmokeTests
{
	[Test]
	public void GameScriptsAreReachableFromEditorTests()
	{
		Assert.IsNotNull(typeof(RotateController));
	}
}
