using Kernel;
using Kernel.Config;
using Kernel.Game;
using Kernel.Runtime;
using UnityEditor;

namespace Alche.Runtime
{
	public partial class ConfigGenApp
	{
		private ConfigGenApp()
		{
			ModuleManager.Instance.RegisterModule(typeof(BasePathModule), () => new EditorPathModule());
			ModuleManager.Instance.RegisterModule(typeof(PlatformModule), () => new UnityEditorPlatformModule());
			ModuleManager.Instance.RegisterModule(typeof(ConfigReaderModule),
				() => new XmlConfigReaderModule(PathManager.Instance.ExternalXmlConfigFolder, false));
		}

		[MenuItem("Test/ConfigGenApp")]
		public static void Test()
		{
			var app = new ConfigGenApp();
			app.Awake();
			while (app.Running)
			{
				app.Update();
				app.LateUpdate();
			}
		}
	}
}
