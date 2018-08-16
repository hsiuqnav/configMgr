namespace Kernel.FSM
{
	public class Event
	{
		public Event(int id)
		{
			Id = id;
		}

		public int Id
		{
			get;
			private set;
		}

		public virtual Event NextEvent
		{
			get
			{
				return null;
			}
		}

		public virtual bool KeepForNextState(object state)
		{
			return false;
		}
	}
}