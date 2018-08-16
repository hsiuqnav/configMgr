namespace Kernel.FSM
{
	public class FiniteEvent : Event
	{
		private static readonly FiniteEvent instance = new FiniteEvent();

		public FiniteEvent()
			: base(-1)
		{
		}

		public static FiniteEvent Instance
		{
			get
			{
				return instance;
			}
		}
	}
}