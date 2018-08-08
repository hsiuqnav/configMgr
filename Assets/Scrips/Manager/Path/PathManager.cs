using System;
using System.IO;
using Kernel.Lang.Extension;
using Kernel.Runtime;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Kernel.Game
{
	/// <summary>
	///     管理游戏中使用的固定路径或者文件名称
	/// </summary>
	public class PathManager : Manager<PathManager>
	{
		private BasePathModule pathModule;

		public string Asset2BundleMap
		{
			get
			{
				return PathModule.Asset2BundleMap;
			}
		}

		public string AssetManifest
		{
			get
			{
				return PathModule.AssetManifest;
			}
		}

		public string BootConfigFolder
		{
			get
			{
				return PathModule.BootConfigFolder;
			}
		}

		public string BootLocaleFolder
		{
			get
			{
				return PathModule.BootLocaleFolder;
			}
		}

		public string BundleFolder
		{
			get
			{
				return PathModule.BundleFolder;
			}
		}

		public string DependencyFilePath
		{
			get
			{
				return PathModule.DependencyFilePath;
			}
		}

		public string ExternalAssetsFolder
		{
			get
			{
				return PathModule.ExternalAssetsFolder;
			}
		}

		public string ExternalBinaryConfig
		{
			get
			{
				return PathModule.ExternalBinaryConfig;
			}
		}

		public string ExternalBinaryFolder
		{
			get
			{
				return PathModule.ExternalBinaryFolder;
			}
		}

		public string ExternalConfigLocaleFolder
		{
			get
			{
				return PathModule.ExternalConfigLocaleFolder;
			}
		}

		public string ExternalExcelExampleFolder
		{
			get
			{
				return PathModule.ExternalExcelExampleFolder;
			}
		}

		public string ExternalFightConfigFolder
		{
			get
			{
				return PathModule.ExternalFightConfigFolder;
			}
		}

		public string ExternalFolder
		{
			get
			{
				return PathModule.ExternalFolder;
			}
		}

		public string ExternalLocaleFolder
		{
			get
			{
				return PathModule.ExternalLocaleFolder;
			}
		}

		public string ExternalSaveFolder
		{
			get
			{
				return PathModule.ExternalSaveFolder;
			}
		}

		public string ExternalLuaBinaryConfig
		{
			get
			{
				return PathModule.ExternalLuaBinaryConfig;
			}
		}

		public string ExternalPlotXmlConfigFolder
		{
			get
			{
				return PathModule.ExternalPlotXmlConfigFolder;
			}
		}

		public string ExternalStoryXmlConfigFolder
		{
			get
			{
				return PathModule.ExternalStoryXmlConfigFolder;
			}
		}

		public string ExternalXmlConfigFolder
		{
			get
			{
				return PathModule.ExternalXmlConfigFolder;
			}
		}

		public string ExternalXmlExampleFolder
		{
			get
			{
				return PathModule.ExternalXmlExampleFolder;
			}
		}

		public string GlobalSaveFile
		{
			get
			{
				return PathModule.GlobalSaveFile;
			}
		}

		public string GlobalShaderBundle
		{
			get
			{
				return PathModule.GlobalShaderBundleName;
			}
		}

		public string InitDataFolder
		{
			get
			{
				return PathModule.InitDataFolder;
			}
		}

		public string InternalAssetsFolder
		{
			get
			{
				return PathModule.InternalAssetsFolder;
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

		public string InternalLocaleFolder
		{
			get
			{
				return PathModule.InternalLocaleFolder;
			}
		}

		public string InternalLuaBinary
		{
			get
			{
				return PathModule.InternalLuaBinary;
			}
		}

		public string InternalLuaBinaryConfig
		{
			get
			{
				return PathModule.InternalLuaBinaryConfig;
			}
		}


		public string InternalPackageContentsFile
		{
			get
			{
				return PathModule.InternalPackageContentsFile;
			}
		}

		public string LuaSourceFolder
		{
			get
			{
				return PathModule.LuaSourceFolder;
			}
		}

		public string MainAssetsFolder
		{
			get
			{
				return PathModule.MainAssetsFolder;
			}
		}

		public string GMFolder
		{
			get
			{
				return PathModule.GMFolder;
			}
		}
		public string[] NonBundleFolders
		{
			get
			{
				return PathModule.NonBundleFolders;
			}
		}

		public string[] InitBundles
		{
			get
			{
				return PathModule.InitBundles;
			}
		}

		public string UpdateDllFolder
		{
			get
			{
				return PathModule.UpdateDllFolder;
			}
		}

		public string[] OverrideInstallProtectFolders
		{
			get
			{
				return PathModule.OverrideInstallProtectFolders;
			}
		}

		public string PrefabFolder
		{
			get
			{
				return PathModule.PrefabFolder;
			}
		}

		public string PrefabPathFile
		{
			get
			{
				return PathModule.PrefabPathFile;
			}
		}

		public string PatchFilePrefix
		{
			get
			{
				return PathModule.PatchFilePrefix;
			}
		}

		public string ShaderBundlePath
		{
			get
			{
				return PathModule.ShaderBundlePath;
			}
		}

		public string ShaderNameMap
		{
			get
			{
				return PathModule.ShaderNameMap;
			}
		}

		public string UnzipRecordFile
		{
			get
			{
				return PathModule.UnzipRecordFile;
			}
		}

		public string InternalVersionFile
		{
			get
			{
				return PathModule.InternalVersionFile;
			}
		}

		private BasePathModule PathModule
		{
			get
			{
#if UNITY_EDITOR
				if (pathModule == null)
				{
#if UNITY_ANDROID
					pathModule = new AndroidPathModule();
#elif UNITY_IOS
					pathModule = new IOSPathModule();
#else
					pathModule = new EditorPathModule();
#endif
				}
#endif
				return pathModule;
			}

			set
			{
				pathModule = value;
			}
		}

		public string AndroidPath(string path, out string prefix)
		{
			return PathModule.AndroidPath(path, out prefix);
		}

		public string ChangeToExternalPath(string innerPath)
		{
			var relativePath = innerPath.Substring(PathModule.InitDataFolder.Length);
			return ToExternalPath(relativePath);
		}

		public string ChangeToInternalPath(string externalPath)
		{
			var relativePath = externalPath.Substring(PathModule.ExternalFolder.Length);
			return ToInternalPath(relativePath);
		}

		public string GetExternalLogFile(int index)
		{
			return PathModule.GetExternalLogFile(index);
		}

		public string GetLocalSaveFile(int serverId, long roleId)
		{
			return PathModule.GetLocalSaveFile(serverId, roleId);
		}

		public bool IsExternalPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			return path.StartsWith(PathModule.ExternalFolder, StringComparison.Ordinal);
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

		public string ToInternalPath(string relativePath)
		{
			return Path.Combine(PathModule.InitDataFolder, relativePath.TrimStart('/', '\\'));
		}

		public string GetNavimeshFilePath(string name)
		{
			return PathModule.GetNavimeshFilePath(name);
		}

		public string InternalKeywordFile
		{
			get
			{
				return PathModule.InternalKeywordFile;
			}
		}

#if UNITY_EDITOR
		public void ChangePlatform(BuildTarget platform)
		{
			switch (platform)
			{
				case BuildTarget.StandaloneWindows:
					pathModule = new EditorPathModule();
					break;
				default:
					throw new ArgumentOutOfRangeException("platform", platform, null);
			}
		}
#endif

		public string ToTemp(string path)
		{
			if (path.IsNullOrEmpty())
			{
				return path;
			}
			return string.Format("{0}.tmp", path);
		}

		public string ToBackup(string path)
		{
			if (path.IsNullOrEmpty())
			{
				return path;
			}
			return string.Format("{0}.backup", path);
		}

		protected override void OnInit()
		{
			PathModule = AddModule<BasePathModule>();
		}
	}
}