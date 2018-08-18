using Kernel.FSM;

namespace Kernel.Runtime
{
	public class GameFSM : FiniteStateMachine<Game>
	{
		public GameFSM(Game content, GameFSMState enterState) : base(content, enterState)
		{

		}

		protected override void OnPreStateChange(State currentState, State nextState)
		{
		}

		protected override void OnPostStateChange(State currentState, State lastState)
		{
		}
	}
}