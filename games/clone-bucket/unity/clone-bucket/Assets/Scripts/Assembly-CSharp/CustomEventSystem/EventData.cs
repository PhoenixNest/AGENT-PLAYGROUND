namespace CustomEventSystem
{
	public class EventData
	{
		public string EventName { get; protected set; }

		public object Sender { get; protected set; }

		public EventData(string eventName, object sender = null)
		{
			EventName = eventName;
			Sender = sender;
		}
	}
}
