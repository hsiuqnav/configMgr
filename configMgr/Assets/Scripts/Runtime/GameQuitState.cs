using Kernel.FSM;

namespace Kernel.Runtime
{
	public class GameQuitState : GameFSMState
	{
		public override bool IsFinished
		{
			get
			{
				return true;
			}
		}

		public GameQuitState(Game content) : base(content)
		{

		}

		protected override void OnEnter(Event e, GameFSM.State lastState)
		{
			Content.Quit();
		}
	}
}