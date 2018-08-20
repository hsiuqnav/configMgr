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
		public GameUnityState(Game content) : base(content)
		{

		}

		protected override void OnEnter(Event e, GameFSM.State lastState)
		{
			ConfigManager.Instance.SetSerializer(new ConfigSerializer());
			ManagerMan.Instance.RegisterManager(PathManager.Instance);
			ManagerMan.Instance.RegisterManager(PlatformManager.Instance);
			ManagerMan.Instance.RegisterManager(WorksManager.Instance);
			ManagerMan.Instance.RegisterManager(ModuleManager.Instance);
			ManagerMan.Instance.RegisterManager(ConfigManager.Instance);
			ManagerMan.Instance.InitAllManagers();
			ManagerMan.Instance.BootAllManagers();
			base.OnEnter(e, lastState);
		}
	}
}
