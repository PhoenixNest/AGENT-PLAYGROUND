using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
	public static T instance { get; protected set; }

	protected virtual void Awake()
	{
		if (instance != null && (object)instance != this)
		{
			Destroy(this);
			return;
		}
		instance = this as T;
	}
}
