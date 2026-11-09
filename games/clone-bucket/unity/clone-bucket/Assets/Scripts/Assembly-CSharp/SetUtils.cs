using UnityEngine;

public class SetUtils
{
	public static Vector2 Vector2SetX(Vector2 v2, float x)
	{
		v2.x = x;
		return v2;
	}

	public static Vector2 Vector2SetY(Vector2 v2, float y)
	{
		v2.y = y;
		return v2;
	}

	public static Vector3 Vector3SetX(Vector3 v3, float x)
	{
		v3.x = x;
		return v3;
	}

	public static Vector3 Vector3SetY(Vector3 v3, float y)
	{
		v3.y = y;
		return v3;
	}

	public static Vector3 Vector3SetZ(Vector3 v3, float z)
	{
		v3.z = z;
		return v3;
	}

	public static Color ColorSetR(Color color, float r)
	{
		color.r = r;
		return color;
	}

	public static Color ColorSetG(Color color, float g)
	{
		color.g = g;
		return color;
	}

	public static Color ColorSetB(Color color, float b)
	{
		color.b = b;
		return color;
	}

	public static Color ColorSetA(Color color, float a)
	{
		color.a = a;
		return color;
	}
}
