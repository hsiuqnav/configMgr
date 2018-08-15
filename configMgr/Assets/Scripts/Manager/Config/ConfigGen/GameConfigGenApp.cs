using GeneratedCode;
using Kernel;
using Kernel.Config;
using Kernel.Game;
using System;
using System.Text;

namespace Alche.Runtime
{
	public class GameConfigGenApp
	{
		private bool readXmlThread;

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

		public GameConfigGenApp(bool readXmlThread)
		{
			this.readXmlThread = readXmlThread;
		}

		private void Start()
		{
			ConfigSerializer serializer = new ConfigSerializer();
			ConfigManager.Instance.SetSerializer(serializer);
			ConfigManager.Instance.ReloadConfigReaderModule(new XmlConfigReaderModule(PathManager.Instance.ExternalXmlConfigFolder, readXmlThread));

			using (BinWriter o = new BinWriter(PlatformManager.Instance.OpenWrite(PathManager.Instance.ExternalBinaryConfig), Encoding.UTF8))
			{
				ConfigManager.Instance.LoadAllConfig();
				serializer.WriteToBinary(o);
			}

			PlatformManager.Instance.ClearDirectory(PathManager.Instance.ExternalXmlExampleFolder);
			new ConfigExampleBuilder().WriteExampleConfig(serializer, PathManager.Instance.ExternalXmlExampleFolder);
		}
	}
}
