using UnityEngine;

[ExecuteInEditMode]
public class FixMobileDepth : MonoBehaviour
{
	private void Awake()
	{
		Camera cam = GetComponent<Camera>();
		if (cam == null)
		{
			return;
		}
		cam.depthTextureMode |= DepthTextureMode.Depth;
	}
}
