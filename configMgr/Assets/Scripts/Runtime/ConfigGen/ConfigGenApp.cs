using Kernel;
using Kernel.Config;
using Kernel.Game;
using Kernel.Runtime;
using System;

namespace Alche.Runtime
{
	public partial class ConfigGenApp : CommandLineApp
	{
		private readonly bool readXmlThread;

		public ConfigGenApp(bool isReadXmlThread)
		{
			readXmlThread = isReadXmlThread;
			ModuleManager.Instance.RegisterModule(typeof(BasePathModule), () => new CommandLinePathModule("../../content"));
			ModuleManager.Instance.RegisterModule(typeof(PlatformModule), () => new CommandLinePlatformModule());
			ModuleManager.Instance.RegisterModule(typeof(ConfigReaderModule),
				() => new XmlConfigReaderModule(PathManager.Instance.ExternalXmlConfigFolder, isReadXmlThread));
		}

		protected override GameFSMState CreateEnterState(Game game)
		{
			return new GameConfigGenState(game, readXmlThread);
		}

		public static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: config <isThread>");
				return;
			}
			bool isThread = Convert.ToBoolean(args[0]);
			ConfigGenApp app = new ConfigGenApp(isThread);
			app.Awake();

			while (app.Running)
			{
				app.Update();
				app.LateUpdate();
			}
		}
	}
}
