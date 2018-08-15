using Kernel.Game;
using Kernel.Lang.Extension;
using Kernel.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Kernel.Config
{
	public class ConfigManager : Manager<ConfigManager>
	{
		private ConfigReaderModule readerModule;
		private XmlConfigReaderModule overrideModule;
		private ConfigSerializerBase serializer;
		private IWork configLoader;

		public IConfigSerializer GetSerializer(Type t)
		{
			if (serializer != null)
			{
				return serializer.Serializers.GetEx(t);
			}
			return null;
		}

		public void SetSerializer(ConfigSerializerBase serializer)
		{
			this.serializer = serializer;
		}

		public void ResetConfigLoader()
		{
			configLoader = null;
		}

		public void AddXmlConfigType(Type type)
		{
			if (overrideModule == null)
			{
				overrideModule = new XmlConfigReaderModule(PathManager.Instance.ExternalXmlConfigFolder);
			}
		}

		public void ReloadConfigReaderModule(ConfigReaderModule reader)
		{
			var old = readerModule;
			if (readerModule != null)
			{
				UnloadModule(readerModule);
				readerModule = null;
			}
			if (readerModule == null)
			{
				readerModule = AddModule(reader);
				readerModule.SetSerializer(serializer);
				readerModule.CopyConfigData(old);
			}
		}

		public void GetConfigRootName(Type type, out string rootName, out string elemName)
		{
			if (TypeUtil.IsDictionary(type))
			{
				elemName = TypeUtil.GetDictionaryValueType(type).Name;
				rootName = elemName + "s";
			}
			else if (TypeUtil.IsSubclassOfDictionary(type))
			{
				elemName = TypeUtil.GetDictionaryValueType(type).Name;
				rootName = "Dictionary" + TypeUtil.GetDictionaryKeyType(type).Name + TypeUtil.GetDictionaryValueType(type).Name;
			}
			else if (TypeUtil.IsArray(type))
			{
				elemName = TypeUtil.GetArrayValueType(type).Name;
				rootName = elemName + "s";
			}
			else if (TypeUtil.IsSubclassOfList(type))
			{
				var valueType = TypeUtil.GetListValueType(type);
				GetConfigRootName(valueType, out rootName, out elemName);

				rootName = TypeUtil.GetGenericTypeName(type) + elemName;
			}
			else if (TypeUtil.IsList(type))
			{
				var valueType = TypeUtil.GetListValueType(type);
				GetConfigRootName(valueType, out rootName, out elemName);

				rootName = TypeUtil.GetGenericTypeName(type) + elemName + "s";
			}
			else
			{
				rootName = type.Name;
				elemName = null;
			}
		}

		public IWork WaitForConfigLoaded()
		{
			if (configLoader == null && readerModule != null)
			{
				var loader = readerModule.LoadConfigAutomatically();
				configLoader = new ConditionWork(() => readerModule != null && readerModule.IsRunning, loader);
			}
			return configLoader;
		}

		public IWork LoadAllBoot()
		{
			return new AtomWork(() => readerModule.LoadAllConfig(true));
		}

		public void LoadAllConfig()
		{
			if (readerModule != null)
			{
				readerModule.LoadAllConfig(false);
			}
		}

		public void LoadAllConst()
		{
			if (readerModule != null)
			{
				readerModule.LoadAllConst();
			}
		}

		public bool SaveXml<TKey, TValue>(XmlConfigFile<TKey, TValue> file, bool constConfig) where TValue : class
		{
			if (overrideModule != null)
			{
				return overrideModule.SaveXml(file, constConfig);
			}
			if (readerModule is XmlConfigReaderModule)
			{
				return (readerModule as XmlConfigReaderModule).SaveXml(file, constConfig);
			}
			return false;
		}

		public string GetDefaultConfigFileName(Type type)
		{
			string fileName = null;
			if (TypeUtil.IsDictionary(type))
			{
				fileName = TypeUtil.GetDictionaryValueType(type).Name + "s";
			}
			else if (TypeUtil.IsArray(type))
			{
				fileName = TypeUtil.GetArrayValueType(type).Name + "s";
			}
			else if (TypeUtil.IsList(type))
			{
				fileName = TypeUtil.GetListValueType(type).Name + "s";
			}
			else
			{
				fileName = type.Name;
			}
			if (fileName.StartsWith("Conf", true, CultureInfo.CurrentCulture))
			{
				return fileName.Substring(4).ToLower();
			}
			return fileName.ToLower();
		}

		public void Clear()
		{
			readerModule.ResetConfigData();
		}

		public void WriteAllConfig(string folder)
		{
			if (readerModule != null)
			{
				readerModule.WriteAllConfig(folder);
			}
		}

		public T GetConfig<T>() where T : class
		{
			if (readerModule != null)
			{
				if (overrideModule != null)
				{
					return overrideModule.LoadConfig<T>() ?? readerModule.LoadConfig<T>();
				}
				return readerModule.LoadConfig<T>();
			}
			return null;
		}

		public T GetConfig<T>(int key) where T : class
		{
			if (readerModule != null)
			{
				if (overrideModule != null)
				{
					return overrideModule.LoadConfig<T, int>(key) ?? readerModule.LoadConfig<T, int>(key);
				}
				return readerModule.LoadConfig<T, int>(key);
			}
			return null;
		}

		public T GetConfig<T>(string key) where T : class
		{
			if (readerModule != null)
			{
				if (overrideModule != null)
				{
					return overrideModule.LoadConfig<T, string>(key) ?? readerModule.LoadConfig<T, string>(key);
				}
				return readerModule.LoadConfig<T, string>(key);
			}
			return null;
		}

		public IEnumerable<TValue> GetConfigs<TKey, TValue>() where TValue : class
		{
			if (readerModule != null)
			{
				if (overrideModule != null)
				{
					var configs = overrideModule.LoadConfigs<TKey, TValue>().ToArray();
					if (configs.Length > 0)
					{
						return configs;
					}
					return readerModule.LoadConfigs<TKey, TValue>();
				}
				return readerModule.LoadConfigs<TKey, TValue>();
			}
			return null;
		}

		public XmlConfigFile<TValue> GetConfigFromFile<TValue>(string xmlFile) where TValue : class
		{
			AddXmlConfigType(typeof(TValue));
			return overrideModule.LoadConfigsFromFile<int, TValue>(xmlFile, true);
		}

		public XmlConfigFile<TValue> GetConfigsFromFile<TKey, TValue>(string xmlFile) where TValue : class
		{
			AddXmlConfigType(typeof(TValue));
			return overrideModule.LoadConfigsFromFile<TKey, TValue>(xmlFile, false);
		}

		public IEnumerable<XmlConfigFile<TValue>> GetConfigsFromFolder<TKey, TValue>(string folder) where TValue : class
		{
			AddXmlConfigType(typeof(TValue));
			return overrideModule.LoadConfigsFromFolder<TKey, TValue>(folder);
		}

		public IXmlConfigDictionary GetXmlDataCollection(Type elemType, Type keyType, bool constConfig)
		{
			return readerModule.GetXmlDataCollection(elemType, keyType, constConfig);
		}

		protected override void OnInit()
		{
			readerModule = AddModule<ConfigReaderModule>(new BootConfigReaderModule(PathManager.Instance.BootConfigFolder,
				PathManager.Instance.InternalBootConfigFolder));
			readerModule.SetSerializer(serializer);

			//LocaleReader.Clear();
			//LocaleReader.AddLocaleFromFolder(PathManager.Instance.BootLocaleFolder);
			//LocaleReader.AddLocaleFromFolder(PathManager.Instance.ChangeToExternalPath(PathManager.Instance.BootLocaleFolder));
		}

		protected override void OnBoot()
		{
			var oldModule = readerModule;
			readerModule = AddModule<ConfigReaderModule>();
			readerModule.SetSerializer(serializer);
			readerModule.CopyConfigData(oldModule);

			//LocaleReader.AddLocaleFromFolder(PathManager.Instance.InternalLocaleFolder);
			//LocaleReader.AddLocaleFromFolder(PathManager.Instance.ChangeToExternalPath(PathManager.Instance.InternalLocaleFolder));
		}

		protected override void OnShutdown()
		{
			UnloadModule(readerModule);
			readerModule = null;
			overrideModule = null;
		}

		internal object GetConfig(Type type)
		{
			if (readerModule != null)
			{
				if (overrideModule != null)
				{
					return overrideModule.LoadConfig(type) ?? readerModule.LoadConfig(type);
				}
				return readerModule.LoadConfig(type);
			}
			return null;
		}
	}
}
