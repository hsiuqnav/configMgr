﻿using System.IO;
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
				if(platformModule == null)
				{
					platformModule = new UnityEditorPlatformModule();
				}
				return platformModule;
			}
			set
			{
				platformModule = value;
			}
		}

		public Stream OpenWrite(string path)
		{
			return PlatformModule.OpenWrite(path);
		}

		public bool CreateParentDirectoryIfNeed(string file)
		{
			return PlatformModule.CreateParentDirectoryIfNeed(file);
		}

		public bool CreateDirectoryIfNeed(string folder)
		{
			return PlatformModule.CreateDirectoryIfNeed(folder);
		}

		public bool ClearDirectory(string path, string extension = null)
		{
			return PlatformModule.ClearDirectory(path, extension);
		}

		public void QuitGame()
		{
			PlatformModule.QuitGame();
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

		public string[] ReadAllLines(string path, Encoding encoding = null)
		{
			return PlatformModule.ReadAllLines(path, encoding);
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

		public Platform Platform
		{
			get
			{
				return PlatformModule.Platform;
			}
		}


		public bool IsEditor
		{
			get
			{
				return Platform == Platform.WINDOWS;
			}
		}

		protected override void OnInit()
		{
			PlatformModule = AddModule<PlatformModule>();

			PlatformModule.InitPlatformInfoAndDevicesInfo();
			PlatformModule.InitPlatformSettings();
		}
	}
}