using UnityEngine;

public class PrivacySettingsPopUp : MonoBehaviour
{
	public void OnClickForgetMe()
	{
		if (TTPManager.Instance != null)
		{
			TTPManager.Instance.ForgetUser();
		}
		else
		{
			Debug.LogWarning("[MM3:Privacy] forget-me not sent, no TTPManager");
		}
	}
}
