using System.IO;
using Kernel.Lang.Extension;
using Kernel.Runtime;

namespace Kernel.Game
{
	public abstract class BasePathModule : Module
	{
		private const string ASSET_BUNDLE_MAP = "{InitDataFolder}/{BundleFolder}/assets/a2b";
		private const string ASSET_MANIFEST = "{ExternalFolder}/{BundleFolder}/assets/assets";
		private const string BOOT_CONFIG_FOLDER = "{InitDataFolder}/bootconfig";
		private const string INTERNAL_BOOT_CONFIG_FOLDER = "{InitDataFolder}/internalbootconfig";
		private const string BOOT_LOCALE_FOLDER = "{InitDataFolder}/bootlocale/{Locale}";
		private const string DEPENDENCY_FILE_PATH = "{InitDataFolder}/{BundleFolder}/assets/dependencies";
		private const string EXTERNAL_ASSETS_FOLDER = "{ExternalFolder}/{BundleFolder}/assets";
		private const string EXTERNAL_BINARY_CONFIG = "{ExternalFolder}/bin/cs.conf";
		private const string EXTERNAL_BINARY_FOLDER = "{ExternalFolder}/bin";
		private const string EXTERNAL_CONFIG_LOCALE_FOLDER = "{ExternalFolder}/../config_locale/{Locale}";
		private const string EXTERNAL_EXCEL_EXAMPLE_FOLDER = "{ExternalFolder}/../config_excel_example";
		private const string EXTERNAL_FIGHT_CONFIG_FOLDER = "{ExternalFolder}/../fight";
		private const string EXTERNAL_SAVE_FOLDER = "{ExternalFolder}/save";
		private const string EXTERNAL_LOCALE_FOLDER = "{ExternalFolder}/locale/{Locale}";
		private const string EXTERNAL_LOG_FILE = "{ExternalFolder}/log/log_{0}.txt";
		private const string EXTERNAL_LUA_BINARY_CONFIG = "{ExternalFolder}/bin/lua.conf";
		private const string EXTERNAL_PLOT_XML_CONFIG_FOLDER = "{ExternalFolder}/../config/plotdialogues";
		private const string EXTERNAL_STORY_XML_CONFIG_FOLDER = "{ExternalFolder}/../config/stories";
		private const string EXTERNAL_UNZIP_RECORD_FILE = "{ExternalFolder}/unzipRecord.txt";
		private const string EXTERNAL_XML_CONFIG_FOLDER = "{ExternalFolder}/../config";
		private const string EXTERNAL_XML_EXAMPLE_FOLDER = "{ExternalFolder}/../config_example";
		private const string GLOBAL_SHADER_BUNDLE_NAME = "globalshaders";
		private const string INTERNAL_ASSETS_FOLDER = "{InitDataFolder}/{BundleFolder}/assets";
		private const string INTERNAL_BINRAY_CONFIG = "{InitDataFolder}/bin/cs.conf";
		private const string INTERNAL_LOCALE_FOLDER = "{InitDataFolder}/locale/{Locale}";
		private const string INTERNAL_LUA_BINARY = "{InitDataFolder}/lua/all.bin";
		private const string INTERNAL_LUA_BINARY_CONFIG = "{InitDataFolder}/bin/lua.conf";
		private const string INTERNAL_NAVIMESH_PATH = "{InitDataFolder}/bin/navimesh/{0}";
		private const string INTERNAL_PACKAGE_CONTENTS_FILE = "assets/content/contents";
		private const string MAIN_ASSETS_FOLDER = "{InitDataFolder}/{BundleFolder}/assets/resources";
		private const string PREFAB_PATH = "{InitDataFolder}/{BundleFolder}/depends/prefab_path";
		private const string SHADER_NAME_MAP = "{InitDataFolder}/{BundleFolder}/assets/shaderNameMap";
		private const string SHADER_PATH = "{InitDataFolder}/{BundleFolder}/assets/resources/shaders.bundle";
		private const string DLL_FOLDER = "update_dll";
		private const string GM_PATH = "{InitDataFolder}/GM";
		private const string EXTERNAL_KEYWORD = "{InitDataFolder}/keyword/keyword.txt";
		private const string INTERNAL_VERSION = "{InitDataFolder}/version";

		private readonly string[] initBundles =
		{
#if !DISABLE_UNITY
			"login",
			"click",
			"click2",
			"globalshaders",
			"dyn_font24",
#endif
		};

		/// <summary>
		/// 覆盖安装时不清理的外部文件夹
		/// </summary>
		private readonly string[] overrideInstallProtectExternalFolders =
		{
			"{ExternalFolder}/log",
		};

		private readonly string prefabFolder = Path.GetFullPath("Assets/Resources").UnixLike();

		private string locale;

		public virtual string PatchFilePrefix
		{
			get
			{
				return "patch/";
			}
		}

		public virtual string Asset2BundleMap
		{
			get
			{
				return Parse(ASSET_BUNDLE_MAP);
			}
		}

		public virtual string AssetManifest
		{
			get
			{
				return Parse(ASSET_MANIFEST);
			}
		}

		public virtual string BootConfigFolder
		{
			get
			{
				return Parse(BOOT_CONFIG_FOLDER);
			}
		}

		public virtual string BootLocaleFolder
		{
			get
			{
				return Parse(BOOT_LOCALE_FOLDER);
			}
		}

		public virtual string InternalBootConfigFolder
		{
			get
			{
				return Parse(INTERNAL_BOOT_CONFIG_FOLDER);
			}
		}

		public abstract string BundleFolder
		{
			get;
		}

		public virtual string DependencyFilePath
		{
			get
			{
				return Parse(DEPENDENCY_FILE_PATH).UnixLike();
			}
		}

		public virtual string ExternalAssetsFolder
		{
			get
			{
				return Parse(EXTERNAL_ASSETS_FOLDER).UnixLike();
			}
		}

		public virtual string ExternalBinaryConfig
		{
			get
			{
				return Parse(EXTERNAL_BINARY_CONFIG);
			}
		}

		public virtual string ExternalBinaryFolder
		{
			get
			{
				return Parse(EXTERNAL_BINARY_FOLDER);
			}
		}

		public virtual string ExternalConfigLocaleFolder
		{
			get
			{
				return Parse(EXTERNAL_CONFIG_LOCALE_FOLDER);
			}
		}

		public virtual string ExternalExcelExampleFolder
		{
			get
			{
				return Parse(EXTERNAL_EXCEL_EXAMPLE_FOLDER);
			}
		}

		public virtual string ExternalFightConfigFolder
		{
			get
			{
				return Parse(EXTERNAL_FIGHT_CONFIG_FOLDER);
			}
		}

		public abstract string ExternalFolder
		{
			get;
		}

		public virtual string ExternalLocaleFolder
		{
			get
			{
				return Parse(EXTERNAL_LOCALE_FOLDER);
			}
		}

		public virtual string ExternalSaveFolder
		{
			get
			{
				return Parse(EXTERNAL_SAVE_FOLDER);
			}
		}

		public virtual string ExternalLuaBinaryConfig
		{
			get
			{
				return Parse(EXTERNAL_LUA_BINARY_CONFIG);
			}
		}

		public virtual string ExternalPlotXmlConfigFolder
		{
			get
			{
				return Parse(EXTERNAL_PLOT_XML_CONFIG_FOLDER);
			}
		}

		public virtual string ExternalStoryXmlConfigFolder
		{
			get
			{
				return Parse(EXTERNAL_STORY_XML_CONFIG_FOLDER);
			}
		}

		public virtual string ExternalXmlConfigFolder
		{
			get
			{
				return Parse(EXTERNAL_XML_CONFIG_FOLDER);
			}
		}

		public virtual string ExternalXmlExampleFolder
		{
			get
			{
				return Parse(EXTERNAL_XML_EXAMPLE_FOLDER);
			}
		}

		public virtual string GlobalSaveFile
		{
			get
			{
				return Path.Combine(ExternalSaveFolder, "global.save");
			}
		}

		public virtual string GlobalShaderBundleName
		{
			get
			{
				return GLOBAL_SHADER_BUNDLE_NAME;
			}
		}

		public abstract string InitDataFolder
		{
			get;
		}


		public virtual string InternalAssetsFolder
		{
			get
			{
				return Parse(INTERNAL_ASSETS_FOLDER).UnixLike();
			}
		}

		public virtual string InternalBinaryConfig
		{
			get
			{
				return Parse(INTERNAL_BINRAY_CONFIG);
			}
		}

		public virtual string InternalLocaleFolder
		{
			get
			{
				return Parse(INTERNAL_LOCALE_FOLDER);
			}
		}

		public virtual string InternalLuaBinary
		{
			get
			{
				return Parse(INTERNAL_LUA_BINARY);
			}
		}

		public virtual string InternalLuaBinaryConfig
		{
			get
			{
				return Parse(INTERNAL_LUA_BINARY_CONFIG);
			}
		}

		public virtual string InternalPackageContentsFile
		{
			get
			{
				return Parse(INTERNAL_PACKAGE_CONTENTS_FILE);
			}
		}

		public virtual string LuaSourceFolder
		{
			get
			{
				return GetFullPath("../lua");
			}
		}

		public virtual string MainAssetsFolder
		{
			get
			{
				return Parse(MAIN_ASSETS_FOLDER).UnixLike();
			}
		}
		public virtual string GMFolder
		{
			get
			{
				return Parse(GM_PATH).UnixLike();
			}
		}

		public virtual string[] NonBundleFolders
		{
			get
			{
				return new string[0];
			}
		}

		public virtual string[] InitBundles
		{
			get
			{
				return initBundles;
			}
		}

		public virtual string UpdateDllFolder
		{
			get
			{
				return DLL_FOLDER;
			}
		}

		public virtual string[] OverrideInstallProtectFolders
		{
			get
			{
				return Parse(overrideInstallProtectExternalFolders);
			}
		}

		public virtual string PrefabFolder
		{
			get
			{
				return prefabFolder;
			}
		}

		public virtual string PrefabPathFile
		{
			get
			{
				return Parse(PREFAB_PATH);
			}
		}

		public virtual string ShaderBundlePath
		{
			get
			{
				return Parse(SHADER_PATH).UnixLike();
			}
		}

		public virtual string ShaderNameMap
		{
			get
			{
				return Parse(SHADER_NAME_MAP).UnixLike();
			}
		}

		public virtual string UnzipRecordFile
		{
			get
			{
				return Parse(EXTERNAL_UNZIP_RECORD_FILE);
			}
		}

		public virtual string InternalVersionFile
		{
			get
			{
				return Parse(INTERNAL_VERSION);
			}
		}

		public virtual string GetExternalLogFile(int index)
		{
			return Parse(EXTERNAL_LOG_FILE, index);
		}

		public virtual string GetLocalSaveFile(int serverId, long roleId)
		{
			return Path.Combine(ExternalSaveFolder, string.Format("{0}_{1}.save", serverId, roleId));
		}

		public virtual string InternalKeywordFile
		{
			get
			{
				return Parse(EXTERNAL_KEYWORD);
			}
		}

		public virtual string AndroidPath(string path, out string prefix)
		{
			prefix = null;
			return path;
		}

		public string GetFullPath(string path)
		{
			return Path.GetFullPath(path);
		}

		public string GetNavimeshFilePath(string name)
		{
			return Parse(INTERNAL_NAVIMESH_PATH, name);
		}

		protected string Parse(string path, params object[] args)
		{
			return StringUtil.Format(path, this, args);
		}

		protected string[] Parse(string[] paths, params object[] args)
		{
			var results = new string[paths.Length];
			for (var i = 0; i < paths.Length; i++)
			{
				results[i] = StringUtil.Format(paths[i], this, args);
			}
			return results;
		}
	}
}