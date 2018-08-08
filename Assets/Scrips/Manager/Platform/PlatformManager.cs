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

		public string ClientVersion
		{
			get
			{
				return PlatformModule.ClientVersion;
			}
		}


		public string DeviceCode
		{
			get
			{
				return PlatformModule.DeviceCode;
			}
		}

		public string DeviceModel
		{
			get
			{
				return PlatformModule.DeviceModel;
			}
		}

		public bool IsBatchMode
		{
			get
			{
				return PlatformModule.IsBatchMode;
			}
		}

		public bool IsCommandLine
		{
			get
			{
				return PlatformModule is CommandLinePlatformModule;
			}
		}

		public bool IsEditor
		{
			get
			{
				return Platform == Platform.WINDOWS || Platform == Platform.OSX;
			}
		}

		public bool IsMobile
		{
			get
			{
				return Platform == Platform.IOS || Platform == Platform.ANDROID;
			}
		}

		public string OperatingSystem
		{
			get
			{
				return PlatformModule.OperatingSystem;
			}
		}

		public Platform Platform
		{
			get
			{
				return PlatformModule.Platform;
			}
		}

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

		public bool ApplyPatch(string oldFile, string newFile, string patchFile)
		{
			return PlatformModule.ApplyPatch(oldFile, newFile, patchFile);
		}

		public bool ClearDirectory(string path, string extension = null)
		{
			return PlatformModule.ClearDirectory(path, extension);
		}

		public bool RemoveDirectory(string path)
		{
			return PlatformModule.RemoveFolder(path);
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

		public string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			return PlatformModule.GetDirectories(path, searchPattern, searchOption);
		}

		public string[] GetDirectories(string path, string searchPattern, bool recursive)
		{
			return PlatformModule.GetDirectories(path, searchPattern, recursive);
		}

		public string[] GetFiles(string path, string searchPattern = "*",
			SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			return PlatformModule.GetFiles(path, searchPattern, searchOption);
		}

		public string[] GetFiles(string path, string searchPattern, bool recursive)
		{
			return PlatformModule.GetFiles(path, searchPattern, recursive);
		}

		public bool FileMove(string source, string dest)
		{
			return PlatformModule.FileMove(source, dest);
		}

		public bool FileReplace(string source, string dest, string backup = null)
		{
			return PlatformModule.FileReplace(source, dest, backup);
		}

		public Stream OpenAppend(string path)
		{
			return PlatformModule.OpenAppend(path);
		}

		public Stream OpenRead(string path)
		{
			return PlatformModule.OpenRead(path);
		}

		public StreamReader OpenText(string path)
		{
			return PlatformModule.OpenText(path);
		}

		public Stream OpenWrite(string path)
		{
			return PlatformModule.OpenWrite(path);
		}

		public void QuitGame()
		{
			PlatformModule.QuitGame();
		}

		public string[] ReadAllLines(string path, Encoding encoding = null)
		{
			return PlatformModule.ReadAllLines(path, encoding);
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

		public bool RemoveFileIfExists(string file)
		{
			return PlatformModule.RemoveFileIfExists(file);
		}

		public void RemoveReadonlyAttribute(string file)
		{
			PlatformModule.RemoveReadonlyAttribute(file);
		}

		public string ReplaceExtension(string file, string ext)
		{
			return PlatformModule.ReplaceExtension(file, ext);
		}

		public bool CheckStorage(long byteSize)
		{
			return PlatformModule.CheckStorage(byteSize);
		}

		public void Restart()
		{
		}

		protected override void OnInit()
		{
			PlatformModule = AddModule<PlatformModule>();

			PlatformModule.InitPlatformInfoAndDevicesInfo();
			PlatformModule.InitPlatformSettings();
		}

		protected override void OnBootLoadFinished()
		{
		}
	}
}