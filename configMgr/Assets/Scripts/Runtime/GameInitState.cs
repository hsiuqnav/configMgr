using GeneratedCode;
using Kernel;
using Kernel.Config;
using Kernel.FSM;
using Kernel.Game;
using Kernel.Runtime;

namespace Alche.Runtime
{
	public class GameInitState : GameFSMState
	{
		private IWork initWork;
		public override bool IsFinished
		{
			get
			{
				return initWork == null || initWork.IsFinished();
			}
		}
		public GameInitState(Game content) : base(content)
		{

		}

		protected override void OnEnter(Event e, GameFSM.State lastState)
		{
			ConfigManager.Instance.SetSerializer(new ConfigSerializer());
			ManagerMan.Instance.RegisterManager(PathManager.Instance);
			ManagerMan.Instance.RegisterManager(PlatformManager.Instance);
			ManagerMan.Instance.RegisterManager(WorksManager.Instance);
			ManagerMan.Instance.RegisterManager(ModuleManager.Instance);
			ManagerMan.Instance.InitAllManagers();

			base.OnEnter(e, lastState);
		}

		protected override GameFSM.State DoTick(float deltaTime)
		{
			// todo jump to next state
			return this;
		}
	}
}
