using Kernel;
using Kernel.Config;
using Kernel.Game;
using Kernel.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

#if UNITY_EDITOR
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
#endif
	}
}
