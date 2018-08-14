using System.IO;
using System.Text;
using Kernel.Runtime;

namespace Kernel
{
	public enum Platform
	{
		UNKNOWN,
		IOS,
		ANDROID,
		WINDOWS,
		OSX,
		CONSOLE
	}

	public class PlatformManager : Manager<PlatformManager>
	{
		private PlatformModule platformModule;

		private PlatformModule PlatformModule
		{
			get
			{
#if UNITY_EDITOR
				if(platformModule == null)
				{
					platformModule = new UnityEditorPlatformModule();
				}
#endif
				return platformModule;
			}
			set
			{
				platformModule = value;
			}
		}

		public bool CreateParentDirectoryIfNeed(string file)
		{
			return PlatformModule.CreateParentDirectoryIfNeed(file);
		}

		public bool FileExists(string path)
		{
			return PlatformModule.FileExists(path);
		}

		public bool DirectoryExists(string path)
		{
			return PlatformModule.DirectoryExists(path);
		}

		public string[] GetFiles(string path, string searchPattern, bool recursive)
		{
			return PlatformModule.GetFiles(path, searchPattern, recursive);
		}

		public Stream OpenRead(string path)
		{
			return PlatformModule.OpenRead(path);
		}

		public StreamReader OpenText(string path)
		{
			return PlatformModule.OpenText(path);
		}

		public string ReadAllText(string path, Encoding encoding = null)
		{
			return PlatformModule.ReadAllText(path, encoding);
		}

		public void WriteAllText(string content, string path, Encoding encoding = null)
		{
			CreateParentDirectoryIfNeed(path);
			PlatformModule.WriteAllText(content, path, encoding);
		}

		public void RemoveReadonlyAttribute(string file)
		{
			PlatformModule.RemoveReadonlyAttribute(file);
		}

		protected override void OnInit()
		{
			PlatformModule = AddModule<PlatformModule>();

			PlatformModule.InitPlatformInfoAndDevicesInfo();
			PlatformModule.InitPlatformSettings();
		}
	}
}