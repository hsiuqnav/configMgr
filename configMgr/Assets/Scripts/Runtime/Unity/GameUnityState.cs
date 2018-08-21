using Config;
using GeneratedCode;
using Kernel;
using Kernel.Config;
using Kernel.FSM;
using Kernel.Game;
using Kernel.Runtime;

namespace Alche.Runtime
{
	public class GameUnityState : GameFSMState
	{
		private CommonWork bootLoadWork;
		public GameUnityState(Game content) : base(content)
		{

		}

		protected override void OnEnter(Event e, GameFSM.State lastState)
		{
			var hero = ConfigManager.Instance.GetConfig<ConfHero>(107);
			if (hero != null)
			{
				Logger.Info(hero.ToString());
			}
		}
	}
}
