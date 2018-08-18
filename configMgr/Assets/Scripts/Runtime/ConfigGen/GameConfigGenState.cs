using GeneratedCode;
using Kernel;
using Kernel.Config;
using Kernel.FSM;
using Kernel.Game;
using Kernel.Runtime;
using System.Text;

namespace Alche.Runtime
{
	public class GameConfigGenState : GameFSMState
	{
		private bool readXmlThread;

		public GameConfigGenState(Game content, bool readXmlThread) : base(content)
		{
			this.readXmlThread = readXmlThread;
		}

		protected override void OnEnter(Event e, GameFSM.State lastState)
		{
			ManagerMan.Instance.RegisterManager(PlatformManager.Instance);
			ManagerMan.Instance.RegisterManager(PathManager.Instance);
			ManagerMan.Instance.RegisterManager(WorksManager.Instance);
			ManagerMan.Instance.RegisterManager(ConfigManager.Instance);
			ManagerMan.Instance.InitAllManagers();
			ManagerMan.Instance.BootAllManagers();
			base.OnEnter(e, lastState);
		}

		protected override FiniteStateMachine<Game, float>.State DoTick(float deltaTime)
		{
			if (!IsFinished)
			{
				Write();
				return new GameQuitState(Content);
			}
			return base.DoTick(deltaTime);
		}

		private void Write()
		{
			ConfigSerializer serializer = new ConfigSerializer();
			ConfigManager.Instance.SetSerializer(serializer);
			ConfigManager.Instance.ReloadConfigReaderModule(new XmlConfigReaderModule(PathManager.Instance.ExternalXmlConfigFolder, readXmlThread));
			//ConfigManager.Instance.ReloadConfigReaderModule(new XmlConfigReaderModule(string.Format("{0}/../config", Path.GetFullPath("../../content")), isReadXmlThread));

			using (BinWriter o = new BinWriter(PlatformManager.Instance.OpenWrite(PathManager.Instance.ExternalBinaryConfig), Encoding.UTF8))
			//using (BinWriter o = new BinWriter(PlatformManager.Instance.OpenWrite(string.Format("{0}/bin/cs.conf", Path.GetFullPath("../../content"))), Encoding.UTF8))
			{
				ConfigManager.Instance.LoadAllConfig();
				serializer.WriteToBinary(o);
			}
			PlatformManager.Instance.ClearDirectory(PathManager.Instance.ExternalXmlExampleFolder);
			new ConfigExampleBuilder().WriteExampleConfig(serializer, PathManager.Instance.ExternalXmlExampleFolder);
			//var externalXmlExampleFolder = string.Format("{0}/../config_example", Path.GetFullPath("../../content"));
			//PlatformManager.Instance.ClearDirectory(externalXmlExampleFolder);
			//new ConfigExampleBuilder().WriteExampleConfig(serializer, externalXmlExampleFolder);
		}
	}
}
