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
			ConfigManager.Instance.ReloadConfigReaderModule(new XmlConfigReaderModule(string.Format("{0}/../config", Path.GetFullPath("../../content")), readXmlThread));

			//using (BinWriter o = new BinWriter(PlatformManager.Instance.OpenWrite(PathManager.Instance.ExternalBinaryConfig), Encoding.UTF8))
			using (BinWriter o = new BinWriter(PlatformManager.Instance.OpenWrite(string.Format("{0}/bin/cs.conf", Path.GetFullPath("../../content"))), Encoding.UTF8))
			{
				ConfigManager.Instance.LoadAllConfig();
				serializer.WriteToBinary(o);
			}
            var externalXmlExampleFolder = string.Format("{0}/../config_example", Path.GetFullPath("../../content"));
			PlatformManager.Instance.ClearDirectory(externalXmlExampleFolder);
			new ConfigExampleBuilder().WriteExampleConfig(serializer, externalXmlExampleFolder);
		}
	}
}
