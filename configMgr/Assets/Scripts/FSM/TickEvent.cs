namespace Kernel.FSM
{
	public class TickEvent<T> : Event
	{
		public T DeltaTime { get; private set; }

		private static readonly TickEvent<T> instance = new TickEvent<T>(default(T));

		public static TickEvent<T> GetInstance(T deltaTime)
		{
			instance.DeltaTime = deltaTime;
			return instance;
		}

		public TickEvent(T deltaTime)
			: base(-1)
		{
			DeltaTime = deltaTime;
		}
	}
}