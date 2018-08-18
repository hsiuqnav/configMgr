using Kernel.FSM;

namespace Kernel.Runtime
{
	public class GameFSMState : GameFSM.State
	{
		protected virtual float LoadingProgress
		{
			get
			{
				return 0;
			}
		}

		protected GameFSMState(Game content) : base(content)
		{

		}

		public override void Enter(Event e, GameFSM.State lastState)
		{
			if (lastState != null)
				Logger.Info("{0} -> {1}", lastState != null ? lastState.GetType().Name : "NONE", GetType().Name);
			base.Enter(e, lastState);
		}

		public override void Exit(Event e, GameFSM.State nextState)
		{
			base.Exit(e, nextState);
		}

		protected override GameFSM.State DoEvent(Event e)
		{
			var evt = e as GameEvent;
			if (evt != null && evt.Name == GameEventType.QUIT)
			{
				return new GameQuitState(Content);
			}
			return this;
		}

		protected override FiniteStateMachine<Game>.State DoTick(float deltaTime)
		{
			return base.DoTick(deltaTime);
		}
	}
}