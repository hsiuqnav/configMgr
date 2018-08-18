using Kernel.FSM;

namespace Kernel.Runtime
{
	public class GameEvent : Event
	{
		public readonly string Name;
		public readonly object Parameter;

		public GameEvent(string name, object parameter = null) : base(0)
		{
			Name = name;
			Parameter = parameter;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}