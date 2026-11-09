using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "Configs/ShopConfig")]
public class ShopConfig : ScriptableObject
{
	public int price = 500;

	public int totalSkinCount;

	public List<SkinBallItem> balls;
}
