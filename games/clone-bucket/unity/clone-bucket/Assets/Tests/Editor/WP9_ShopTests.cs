using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class WP9_ShopTests
{
	private readonly List<Object> _created = new List<Object>();

	[TearDown]
	public void TearDown()
	{
		for (int i = 0; i < _created.Count; i++)
		{
			if (_created[i] != null)
			{
				Object.DestroyImmediate(_created[i]);
			}
		}

		_created.Clear();
	}

	private T Track<T>(T obj) where T : Object
	{
		_created.Add(obj);
		return obj;
	}

	[Test]
	public void ShopConfigDefaultPriceIs500()
	{
		var config = Track(ScriptableObject.CreateInstance<ShopConfig>());
		Assert.AreEqual(500, config.price);
	}

	[Test]
	public void ShopConfigAssetsAllCost500()
	{
		string[] guids = AssetDatabase.FindAssets("t:ShopConfig");
		if (guids.Length == 0)
		{
			Assert.Inconclusive("No ShopConfig assets found in the project.");
		}

		foreach (string guid in guids)
		{
			var config = AssetDatabase.LoadAssetAtPath<ShopConfig>(AssetDatabase.GUIDToAssetPath(guid));
			Assert.AreEqual(500, config.price, config.name);
		}
	}

	[Test]
	public void SkinOwnedAndLockedTransitions()
	{
		var skinNames = new List<string> { "Ball1", "Ball2", "Ball3" };
		var owned = new HashSet<string> { "Ball1" };

		Assert.IsTrue(SkinManager.IsOwnedName(owned, "Ball1"));
		Assert.IsFalse(SkinManager.IsOwnedName(owned, "Ball2"));
		Assert.IsFalse(SkinManager.IsOwnedName(owned, null));
		CollectionAssert.AreEqual(new[] { 1, 2 }, SkinManager.LockedIndices(skinNames, owned));

		Assert.IsTrue(SkinManager.MarkOwnedName(owned, skinNames, "Ball2"), "first unlock succeeds");
		Assert.IsFalse(SkinManager.MarkOwnedName(owned, skinNames, "Ball2"), "second unlock is a no-op");
		Assert.IsFalse(SkinManager.MarkOwnedName(owned, skinNames, "Ball9"), "unknown skin is not owned");
		CollectionAssert.AreEqual(new[] { 2 }, SkinManager.LockedIndices(skinNames, owned));

		Assert.IsTrue(SkinManager.MarkOwnedName(owned, skinNames, "Ball3"));
		CollectionAssert.IsEmpty(SkinManager.LockedIndices(skinNames, owned));
	}

	[Test]
	public void SkinDefaultIsBall1()
	{
		Assert.AreEqual("Ball1", SkinManager.DefaultSkinName);
	}

	[Test]
	public void ShopVariantParsesRemoteStringAndDefaultsToZero()
	{
		Assert.AreEqual(0, SkinManager.ParseVariant(null));
		Assert.AreEqual(0, SkinManager.ParseVariant(""));
		Assert.AreEqual(0, SkinManager.ParseVariant("abc"));
		Assert.AreEqual(2, SkinManager.ParseVariant("2"));
	}

	[Test]
	public void ShopVariantOutOfRangeFallsBackToFirstConfig()
	{
		var a = Track(ScriptableObject.CreateInstance<ShopConfig>());
		var b = Track(ScriptableObject.CreateInstance<ShopConfig>());
		var configs = new[] { a, b };
		Assert.AreSame(a, SkinManager.PickVariant(configs, 0));
		Assert.AreSame(b, SkinManager.PickVariant(configs, 1));
		Assert.AreSame(a, SkinManager.PickVariant(configs, 5), "index past the end uses ShopConfig");
		Assert.AreSame(a, SkinManager.PickVariant(configs, -1), "negative index uses ShopConfig");
		Assert.IsNull(SkinManager.PickVariant(new ShopConfig[0], 0));
	}
	[Test]
	public void GetSchemeResolvesFiveIds()
	{
		var config = Track(ScriptableObject.CreateInstance<ColorSchemesConfig>());
		config.schemes = new List<ColorSchemeConfig>();
		string[] ids = { "Grey", "Black", "Blue", "Blue_Old", "Orange" };
		foreach (string id in ids)
		{
			var scheme = Track(ScriptableObject.CreateInstance<ColorSchemeConfig>());
			scheme.id = id;
			config.schemes.Add(scheme);
		}

		foreach (string id in ids)
		{
			Assert.IsNotNull(config.GetScheme(id), id);
			Assert.AreEqual(id, config.GetScheme(id).id);
		}

		Assert.IsNull(config.GetScheme("Red"));
		Assert.IsNull(config.GetScheme(null));
	}

	[Test]
	public void GetSchemeResolvesRealAssets()
	{
		string[] guids = AssetDatabase.FindAssets("t:ColorSchemesConfig");
		if (guids.Length == 0)
		{
			Assert.Inconclusive("No ColorSchemesConfig asset found in the project.");
		}

		var config = AssetDatabase.LoadAssetAtPath<ColorSchemesConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
		Assert.AreEqual(5, config.schemes.Count);
		foreach (string id in new[] { "Grey", "Black", "Blue", "Blue_Old", "Orange" })
		{
			Assert.IsNotNull(config.GetScheme(id), id);
		}
	}

	[Test]
	public void GetMaterialsReturnsTheListForEachType()
	{
		var scheme = Track(ScriptableObject.CreateInstance<ColorSchemeConfig>());
		scheme.ballsMaterials = new List<Material>();
		scheme.mazeWallsMaterials = new List<Material>();
		scheme.mazeBackgroundMaterials = new List<Material>();
		scheme.stageBackgroundMaterials = new List<Material>();
		scheme.bucketMaterials = new List<Material>();
		scheme.bucketGuidesMaterials = new List<Material>();
		scheme.bucketStandMaterials = new List<Material>();

		Assert.AreSame(scheme.ballsMaterials, scheme.GetMaterials(ColorSchemeItemType.Balls));
		Assert.AreSame(scheme.mazeWallsMaterials, scheme.GetMaterials(ColorSchemeItemType.MazeWalls));
		Assert.AreSame(scheme.mazeBackgroundMaterials, scheme.GetMaterials(ColorSchemeItemType.MazeBackground));
		Assert.AreSame(scheme.stageBackgroundMaterials, scheme.GetMaterials(ColorSchemeItemType.StageBackground));
		Assert.AreSame(scheme.bucketMaterials, scheme.GetMaterials(ColorSchemeItemType.Bucket));
		Assert.AreSame(scheme.bucketGuidesMaterials, scheme.GetMaterials(ColorSchemeItemType.BucketGuides));
		Assert.AreSame(scheme.bucketStandMaterials, scheme.GetMaterials(ColorSchemeItemType.BucketStand));
		Assert.IsNull(scheme.GetMaterials((ColorSchemeItemType)99));
	}

	[Test]
	public void ItemViewShowSetsTheSlotMaterial()
	{
		var go = new GameObject("WP9_ItemView");
		_created.Add(go);
		var meshRenderer = go.AddComponent<MeshRenderer>();
		var view = go.AddComponent<ColorSchemeItemView>();

		Material material = Track(new Material(Shader.Find("Standard")));
		view.Show(material);

		Assert.AreSame(material, meshRenderer.sharedMaterial);
		Assert.IsTrue(view.IsColorSchemeApplied);

		view.Show(null);
		Assert.IsFalse(view.IsColorSchemeApplied, "null material clears the applied flag");
		Assert.AreSame(material, meshRenderer.sharedMaterial, "null without hide keeps the last material");
	}
}
