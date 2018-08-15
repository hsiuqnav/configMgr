using System;
using System.IO;
using Kernel.Runtime;

namespace Kernel.Game
{
	/// <summary>
	///     管理游戏中使用的固定路径或者文件名称
	/// </summary>
	public class PathManager : Manager<PathManager>
	{
		private BasePathModule pathModule;

		public string ExternalExcelExampleFolder
		{
			get
			{
				return PathModule.ExternalExcelExampleFolder;
			}
		}


		public string BootConfigFolder
		{
			get
			{
				return PathModule.BootConfigFolder;
			}
		}

		public string ExternalLocaleFolder
		{
			get
			{
				return PathModule.ExternalLocaleFolder;
			}
		}

		public string ExternalXmlConfigFolder
		{
			get
			{
				return PathModule.ExternalXmlConfigFolder;
			}
		}

		public string InternalBinaryConfig
		{
			get
			{
				return PathModule.InternalBinaryConfig;
			}
		}

		public string InternalBootConfigFolder
		{
			get
			{
				return PathModule.InternalBootConfigFolder;
			}
		}

		private BasePathModule PathModule
		{
			get
			{
				if (pathModule == null)
				{
					pathModule = new EditorPathModule();
				}
				return pathModule;
			}

			set
			{
				pathModule = value;
			}
		}

		public string ChangeToExternalPath(string innerPath)
		{
			var relativePath = innerPath.Substring(PathModule.InitDataFolder.Length);
			return ToExternalPath(relativePath);
		}

		public bool IsInternalPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			return path.StartsWith(PathModule.InitDataFolder, StringComparison.Ordinal);
		}

		public string ToExternalPath(string relativePath)
		{
			return Path.Combine(PathModule.ExternalFolder, relativePath.TrimStart('/', '\\'));
		}

		protected override void OnInit()
		{
			PathModule = AddModule<BasePathModule>();
		}
	}
}