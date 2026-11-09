using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomEventSystem
{
	public static class EventManager
	{
		private static Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

		private static Dictionary<string, Action<EventData>> eventDataDictionary = new Dictionary<string, Action<EventData>>();

		private static Dictionary<string, Delegate> genericEventDictionary = new Dictionary<string, Delegate>();

		public static void AddListener(string eventName, Action listener)
		{
			Add(eventDictionary, eventName, listener);
		}

		public static void RemoveListener(string eventName, Action listener)
		{
			Remove(eventDictionary, eventName, listener);
		}

		public static void TriggerEvent(string eventName)
		{
			if (eventDictionary.TryGetValue(eventName, out Action action))
			{
				action.Invoke();
			}
		}

		public static void AddListener(string eventName, Action<EventData> listener)
		{
			Add(eventDataDictionary, eventName, listener);
		}

		public static void RemoveListener(string eventName, Action<EventData> listener)
		{
			Remove(eventDataDictionary, eventName, listener);
		}

		public static void TriggerEvent(string eventName, EventData eventData)
		{
			if (eventDataDictionary.TryGetValue(eventName, out Action<EventData> action))
			{
				action.Invoke(eventData);
			}
		}

		public static void AddListener<T>(string eventName, Action<T> listener)
		{
			AddGeneric(eventName, listener);
		}

		public static void AddListener<T1, T2>(string eventName, Action<T1, T2> listener)
		{
			AddGeneric(eventName, listener);
		}

		public static void AddListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
		{
			AddGeneric(eventName, listener);
		}

		public static void RemoveListener<T>(string eventName, Action<T> listener)
		{
			RemoveGeneric(eventName, listener);
		}

		public static void RemoveListener<T1, T2>(string eventName, Action<T1, T2> listener)
		{
			RemoveGeneric(eventName, listener);
		}

		public static void RemoveListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
		{
			RemoveGeneric(eventName, listener);
		}

		public static void TriggerEvent<T>(string eventName, T param)
		{
			if (genericEventDictionary.TryGetValue(eventName, out Delegate d) && d is Action<T> action)
			{
				action.Invoke(param);
			}
		}

		public static void TriggerEvent<T1, T2>(string eventName, T1 param1, T2 param2)
		{
			if (genericEventDictionary.TryGetValue(eventName, out Delegate d) && d is Action<T1, T2> action)
			{
				action.Invoke(param1, param2);
			}
		}

		public static void TriggerEvent<T1, T2, T3>(string eventName, T1 param1, T2 param2, T3 param3)
		{
			if (genericEventDictionary.TryGetValue(eventName, out Delegate d) && d is Action<T1, T2, T3> action)
			{
				action.Invoke(param1, param2, param3);
			}
		}

		public static void ClearAllListeners()
		{
			eventDictionary.Clear();
			eventDataDictionary.Clear();
			genericEventDictionary.Clear();
		}

		private static void Add<TDelegate>(Dictionary<string, TDelegate> dictionary, string eventName, TDelegate listener)
			where TDelegate : Delegate
		{
			if (listener == null)
			{
				return;
			}
			if (dictionary.TryGetValue(eventName, out TDelegate existing))
			{
				if (Array.IndexOf(existing.GetInvocationList(), listener) >= 0)
				{
					return;
				}
				dictionary[eventName] = (TDelegate)Delegate.Combine(existing, listener);
			}
			else
			{
				dictionary[eventName] = listener;
			}
		}

		private static void Remove<TDelegate>(Dictionary<string, TDelegate> dictionary, string eventName, TDelegate listener)
			where TDelegate : Delegate
		{
			if (listener == null || !dictionary.TryGetValue(eventName, out TDelegate existing))
			{
				return;
			}
			var remaining = (TDelegate)Delegate.Remove(existing, listener);
			if (remaining == null)
			{
				dictionary.Remove(eventName);
			}
			else
			{
				dictionary[eventName] = remaining;
			}
		}

		private static void AddGeneric(string eventName, Delegate listener)
		{
			if (listener == null)
			{
				return;
			}
			if (genericEventDictionary.TryGetValue(eventName, out Delegate existing))
			{
				if (existing.GetType() != listener.GetType())
				{
					Debug.LogError("[MM3:Events] Event '" + eventName + "' already has listeners with another signature.");
					return;
				}
				if (Array.IndexOf(existing.GetInvocationList(), listener) >= 0)
				{
					return;
				}
				genericEventDictionary[eventName] = Delegate.Combine(existing, listener);
			}
			else
			{
				genericEventDictionary[eventName] = listener;
			}
		}

		private static void RemoveGeneric(string eventName, Delegate listener)
		{
			if (listener == null || !genericEventDictionary.TryGetValue(eventName, out Delegate existing))
			{
				return;
			}
			Delegate remaining = Delegate.Remove(existing, listener);
			if (remaining == null)
			{
				genericEventDictionary.Remove(eventName);
			}
			else
			{
				genericEventDictionary[eventName] = remaining;
			}
		}
	}
}
