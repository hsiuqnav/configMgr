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
			var action = ConfigManager.Instance.GetConfig<ConfAction>("Jump");
			if (action != null)
			{
				Logger.Info(action.ToString());
			}
			var heroSkin = ConfigManager.Instance.GetConfig<ConfHeroSkin>(2);
			if (heroSkin != null)
			{
				Logger.Info(heroSkin.ToString());
			}
		}
	}
}
