using System.IO;
using Kernel.Lang.Extension;
using Kernel.Runtime;

namespace Kernel.Game
{
	public abstract class BasePathModule : Module
	{
		private const string BOOT_CONFIG_FOLDER = "{InitDataFolder}/bootconfig";
		private const string INTERNAL_BOOT_CONFIG_FOLDER = "{InitDataFolder}/internalbootconfig";
		private const string EXTERNAL_EXCEL_EXAMPLE_FOLDER = "{ExternalFolder}/../config_excel_example";
		private const string EXTERNAL_SAVE_FOLDER = "{ExternalFolder}/save";
		private const string EXTERNAL_LOCALE_FOLDER = "{ExternalFolder}/locale/{Locale}";
		private const string EXTERNAL_LOG_FILE = "{ExternalFolder}/log/log_{0}.txt";
		private const string EXTERNAL_XML_CONFIG_FOLDER = "{ExternalFolder}/../config";
		private const string INTERNAL_BINRAY_CONFIG = "{InitDataFolder}/bin/cs.conf";

		public virtual string BootConfigFolder
		{
			get
			{
				return Parse(BOOT_CONFIG_FOLDER);
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

		public virtual string ExternalXmlConfigFolder
		{
			get
			{
				return Parse(EXTERNAL_XML_CONFIG_FOLDER);
			}
		}

		public virtual string ExternalExcelExampleFolder
		{
			get
			{
				return Parse(EXTERNAL_EXCEL_EXAMPLE_FOLDER);
			}
		}


		public abstract string InitDataFolder
		{
			get;
		}

		public virtual string InternalBinaryConfig
		{
			get
			{
				return Parse(INTERNAL_BINRAY_CONFIG);
			}
		}

		public virtual string GetExternalLogFile(int index)
		{
			return Parse(EXTERNAL_LOG_FILE, index);
		}

		public string GetFullPath(string path)
		{
			return Path.GetFullPath(path);
		}

		protected string Parse(string path, params object[] args)
		{
			return StringUtil.Format(path, this, args);
		}
	}
}