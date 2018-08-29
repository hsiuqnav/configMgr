using Kernel;
using Kernel.Game;
using Kernel.Runtime;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Alche.Runtime
{
	public partial class CodeGenApp
	{

		private CodeGenApp(string targetFolder)
		{
			this.targetFolder = targetFolder;
			ModuleManager.Instance.RegisterModule(typeof(BasePathModule), () => new EditorPathModule());
			ModuleManager.Instance.RegisterModule(typeof(PlatformModule), () => new UnityEditorPlatformModule());
		}

#if UNITY_EDITOR
		[MenuItem("Test/CodeGenApp")]
		public static void Test()
		{
			var tempFolder = Path.GetFullPath("../temp/codeGen");
			PlatformManager.Instance.CreateDirectoryIfNeed(tempFolder);
			var app = new CodeGenApp(tempFolder);
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
