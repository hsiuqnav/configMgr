using GeneratedCode;
using Kernel;
using Kernel.Config;
using Kernel.Game;
using Kernel.Runtime;
using System;
using System.IO;
using System.Text;

namespace Alche.Runtime
{
	public class GameConfigGenApp
	{
		private bool isReadXmlThread;

		public static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				System.Console.WriteLine("Usage: config <isThread>");
				return;
			}
			bool isThread = Convert.ToBoolean(args[0]);
			var app = new GameConfigGenApp(isThread);
			app.Start();
		}

		public GameConfigGenApp(bool isReadXmlThread)
		{
			this.isReadXmlThread = isReadXmlThread;
			ModuleManager.Instance.RegisterModule(typeof(BasePathModule), () => new CommandLinePathModule("../../content"));
			ModuleManager.Instance.RegisterModule(typeof(PlatformModule), () => new CommandLinePlatformModule());
			ModuleManager.Instance.RegisterModule(typeof(ConfigReaderModule),
				() => new XmlConfigReaderModule(PathManager.Instance.ExternalXmlConfigFolder, isReadXmlThread));
		}

		private void Start()
		{
			ManagerMan.Instance.RegisterManager(PlatformManager.Instance);
			ManagerMan.Instance.RegisterManager(PathManager.Instance);
			ManagerMan.Instance.RegisterManager(WorksManager.Instance);
			ManagerMan.Instance.RegisterManager(ConfigManager.Instance);
			ManagerMan.Instance.InitAllManagers();
			ManagerMan.Instance.BootAllManagers();

			ConfigSerializer serializer = new ConfigSerializer();
			ConfigManager.Instance.SetSerializer(serializer);
			ConfigManager.Instance.ReloadConfigReaderModule(new XmlConfigReaderModule(PathManager.Instance.ExternalXmlConfigFolder, isReadXmlThread));
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
