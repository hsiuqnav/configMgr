using Kernel.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;

namespace Kernel.Config
{
	public class XmlConfigReaderModule : ConfigReaderModule
	{
		private readonly string folder;
		private readonly bool isThread;

		public XmlConfigReaderModule(string folder, bool isThread = false)
		{
			this.folder = folder;
			this.isThread = isThread;
		}

		public override IWork LoadConfigAutomatically()
		{
			return new AtomWork(() => LoadAllConfig());
		}

		public override void WriteAllConfig(string targetFolder)
		{
			WriteXML(Serializer.Fields, targetFolder);
		}

		public override void LoadAllConfig(bool bootOnly = false)
		{
			ResetConfigData();
			ReadXML(Serializer.Fields, (name, type) => GetFilesFromFolder(folder, name, ".xml"));
		}

		public override void LoadAllConst()
		{
			LoadAllConfig();
		}

		public override T LoadConfig<T>()
		{
			var conf = base.LoadConfig<T>();
			if (conf is IXmlConfigDictionary)
			{
				return (conf as IXmlConfigDictionary).OriginData as T;
			}
			return conf;
		}

		internal override object LoadConfig(Type type)
		{
			var conf = base.LoadConfig(type);
			if (conf is IXmlConfigDictionary)
			{
				return (conf as IXmlConfigDictionary).OriginData;
			}
			return conf;
		}

		public override IEnumerable<TValue> LoadConfigs<TKey, TValue>()
		{
			var data = GetConfigCollection(typeof(TValue)) as IDictionary;
			if (data != null)
			{
				foreach (var o in data.Values)
				{
					if (o is TValue)
						yield return o as TValue;
				}
			}
		}

		public XmlConfigFile<TValue> LoadConfigsFromFile<TKey, TValue>(string xmlFile, bool constConfig) where TValue : class
		{
			Type elemType = typeof(TValue);

			IXmlConfigDictionary<TKey> xmlDict = GetXmlDataCollection(typeof(TValue), typeof(TKey), constConfig) as IXmlConfigDictionary<TKey>;

			if (xmlDict.ContainsFile(xmlFile)) return new XmlConfigFile<TKey, TValue>(xmlFile, xmlDict.GetFromFile<TValue>(xmlFile), constConfig);
			ReadXML(elemType, (name, type) => GetXmlFile(elemType, type, xmlFile));
			return new XmlConfigFile<TKey, TValue>(xmlFile, xmlDict.GetFromFile<TValue>(xmlFile), constConfig);
		}

		public IEnumerable<XmlConfigFile<TValue>> LoadConfigsFromFolder<TKey, TValue>(string folder) where TValue : class
		{
			List<XmlConfigFile<TValue>> list = new List<XmlConfigFile<TValue>>();
			if (PlatformManager.Instance.DirectoryExists(folder))
			{
				foreach (var file in PlatformManager.Instance.GetFiles(folder, "*.xml", true))
				{
					if (!file.Contains("_history"))
					{
						var f = LoadConfigsFromFile<TKey, TValue>(file, false);
						if (f != null) list.Add(f);
					}
				}
			}
			if (PlatformManager.Instance.FileExists(folder + ".xml"))
			{
				var f = LoadConfigsFromFile<TKey, TValue>(folder + ".xml", false);
				if (f != null) list.Add(f);
			}
			return list;
		}

		public void ClearConfigsFromFile<TKey, TValue>(string file, bool constConfig) where TValue : class
		{
			IXmlConfigDictionary<TKey> xmlDict = GetXmlDataCollection(typeof(TValue), typeof(TKey), constConfig) as IXmlConfigDictionary<TKey>;
			if (xmlDict != null) xmlDict.ClearFileData(file);
		}

		public bool SaveXml<TKey, TValue>(XmlConfigFile<TKey, TValue> file, bool constConfig) where TValue : class
		{
			IXmlConfigDictionary xmlDict = GetXmlDataCollection(typeof(TValue), typeof(TKey), constConfig);

			if (!string.IsNullOrEmpty(file.OriginPath) && file.OriginPath != file.Path)
			{
				xmlDict.DeleteFile(file.OriginPath);
			}
			if (!string.IsNullOrEmpty(file.Path))
			{
				return xmlDict.SaveToFile<TValue>(file.Path, file.Data);
			}
			return true;
		}

		private IEnumerable<string> GetXmlFile(Type targetType, Type configType, string xmlFile)
		{
			bool match = false;
			if (TypeUtil.IsDictionary(configType))
			{
				match = TypeUtil.GetDictionaryValueType(configType) == targetType;
			}
			else if (TypeUtil.IsArray(targetType))
			{
				match = TypeUtil.GetArrayValueType(configType) == targetType;
			}
			if (match || configType == targetType)
			{
				yield return xmlFile;
			}
		}

		private void ReadXML(Type elemType, Func<string, Type, IEnumerable<string>> getFiles)
		{
			var attribute = TypeUtil.GetAttribute<ConfigAttribute>(elemType);
			Type configType = attribute.GetConfigType(elemType);
			DictionaryConfigAttribute dictAttr = attribute as DictionaryConfigAttribute;

			string name = attribute.Name ?? ConfigManager.Instance.GetDefaultConfigFileName(configType);
			foreach (var file in getFiles(name, configType))
			{
				AppendData(configType, elemType, name,
					dictAttr != null ? elemType.GetMember(dictAttr.Key).FirstOrDefault() : null,
					ReadXMLFromFile(file, configType),
					file);
			}
		}

		private void ReadXML(IEnumerable<ConfigFieldInfo> configFields, Func<string, Type, IEnumerable<string>> getFiles)
		{
			if (isThread == true)
			{
				List<Thread> threads = new List<Thread>();
				foreach (ConfigFieldInfo conf in configFields)
				{
					IEnumerable<string> fileList = getFiles(conf.Name, conf.ConfigType);
					foreach (string file in fileList)
					{
						var thread = new Thread(() =>
						{
							Logger.Trace("Read XML " + file);
							Array xmlList = ReadXMLFromFile(file, conf.ConfigType, true);
							AppendData(conf.ConfigType, conf.ElemType, conf.Name, conf.KeyField, xmlList, file);
						});
						thread.Start();
						threads.Add(thread);
					}
				}

				foreach (Thread thread in threads)
				{
					thread.Join();
				}
			}
			else
			{
				foreach (ConfigFieldInfo conf in configFields)
				{
					IEnumerable<string> fileList = getFiles(conf.Name, conf.ConfigType);
					foreach (string file in fileList)
					{
						Logger.Trace("Read XML " + file);
						Array xmlList = ReadXMLFromFile(file, conf.ConfigType, false);
						AppendData(conf.ConfigType, conf.ElemType, conf.Name, conf.KeyField, xmlList, file);
					}
				}
			}
		}

		private Array ReadXMLFromFile(string file, Type fieldType, bool useMultiThread = false)
		{
			var externalFile = PriorityPathManager.Instance.ExternalPathFirst(file);
			if (!PlatformManager.Instance.FileExists(externalFile))
			{
				Logger.Warn("ReadData {0} for type {1}: file not found", externalFile, fieldType);
				return null;
			}

			string rootName = TypeUtil.GetSerializeTypeName(fieldType);

			Type valueType = TypeUtil.GetCollectionValueType(fieldType);

			XmlAttributeOverrides overrides = null;
			if (valueType != null && valueType.BaseType != null && valueType.BaseType.Name == valueType.Name)
			{
				overrides = new XmlAttributeOverrides();
				overrides.Add(valueType, new XmlAttributes { XmlType = new XmlTypeAttribute(valueType.Name + "Sub") });
			}

			if (TypeUtil.IsDictionary(fieldType))
			{
				Type arrayType = valueType.MakeArrayType();
				return (Array)XmlUtil.Read(externalFile, arrayType, rootName, useMultiThread, overrides);
			}
			else
			{
				return new[] { XmlUtil.Read(externalFile, fieldType, rootName, useMultiThread, overrides) };
			}
		}

		private void AppendData(Type collectionType, Type elemType, string name, MemberInfo keyField, Array data, string xmlPath)
		{
			if (TypeUtil.IsDictionary(collectionType))
			{
				IXmlConfigDictionary xmlDict = GetXmlDataCollection(elemType, TypeUtil.GetDictionaryKeyType(collectionType), false);
				if (xmlDict != null)
					xmlDict.Add(xmlPath, data, keyField);
			}
			else
			{
				IXmlConfigDictionary xmlDict = GetXmlDataCollection(elemType, typeof(int), true);
				if (xmlDict != null)
					xmlDict.Add(xmlPath, 0, (data as IList)[0]);
			}
		}

		private string GetXMLFile(string folder, string name)
		{
			return Path.Combine(folder, name.ToLower() + ".xml");
		}

		private void WriteXML(IEnumerable<ConfigFieldInfo> configFields, string folder)
		{
			foreach (var field in configFields)
			{
				var data = GetConfigCollection(field.ElemType);
				WriteXML(GetXMLFile(folder, field.Name), data, field.ConfigType);
			}
		}

		public bool WriteXML(string path, object data, Type type)
		{
			if (data == null) return false;

			string rootName = TypeUtil.GetSerializeTypeName(type);

			Type valueType = TypeUtil.GetCollectionValueType(type);

			XmlAttributeOverrides overrides = null;
			if (valueType != null && valueType.BaseType != null && valueType.BaseType.Name == valueType.Name)
			{
				overrides = new XmlAttributeOverrides();
				overrides.Add(valueType, new XmlAttributes { XmlType = new XmlTypeAttribute(valueType.Name + "Sub") });
			}

			if (TypeUtil.IsDictionary(type))
			{
				Type arrayType = valueType.MakeArrayType();
				IDictionary dict = data as IDictionary;

				if (dict == null) return false;
				Array array = Array.CreateInstance(valueType, dict.Count);
				int i = 0;
				foreach (object elem in dict.Values)
				{
					array.SetValue(elem, i++);
				}

				return XmlUtil.Write(path, array, arrayType, rootName, overrides);
			}
			else
			{
				return XmlUtil.Write(path, data, type, rootName);
			}
		}
	}
}