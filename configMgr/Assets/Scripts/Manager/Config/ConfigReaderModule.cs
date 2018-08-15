using Kernel.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kernel.Config
{
	public abstract class ConfigReaderModule : Module
	{
		public abstract IWork LoadConfigAutomatically();
		public abstract void WriteAllConfig(string targetFolder);
		public abstract void LoadAllConfig(bool bootOnly = false);
		public abstract void LoadAllConst();

		private static System.Object thread_locker = new System.Object();

		private Dictionary<Type, object> configData = new Dictionary<Type, object>();

		private ConfigSerializerBase serializer;


		protected ConfigSerializerBase Serializer
		{
			get
			{
				return serializer;
			}
		}

		public void SetSerializer(ConfigSerializerBase serializer)
		{
			this.serializer = serializer;
		}

		public IXmlConfigDictionary GetXmlDataCollection(Type elemType, Type keyType, bool constConfig)
		{
			lock (thread_locker)
			{
				object collection;
				if (configData.TryGetValue(elemType, out collection))
				{
					if (!(collection is IXmlConfigDictionary))
					{
						collection = CreateXmlDictionaryFromType(keyType, collection, constConfig);
						if (collection == null)
							return null;
						configData[elemType] = collection;
					}
					return collection as IXmlConfigDictionary;
				}
				else
				{
					collection = CreateXmlDictionaryFromType(keyType, null, constConfig);
					if (collection == null)
						return null;
					configData[elemType] = collection;
					return collection as IXmlConfigDictionary;
				}
			}
		}

		public virtual T LoadConfig<T>() where T : class
		{
			lock (thread_locker)
			{
				Type elemType = typeof(T);
				if (configData.ContainsKey(elemType))
				{
					return configData[elemType] as T;
				}
				return default(T);
			}
		}

		public virtual T LoadConfig<T, TKey>(TKey key) where T : class
		{
			lock (thread_locker)
			{
				Type elemType = typeof(T);
				if (configData.ContainsKey(elemType))
				{
					IDictionary<TKey, object> confs = configData[elemType] as IDictionary<TKey, object>;
					if (confs != null && confs.ContainsKey(key))
					{
						return (T)confs[key];
					}
				}
				return default(T);
			}
		}

		internal virtual object LoadConfig(Type type)
		{
			lock (thread_locker)
			{
				if (configData.ContainsKey(type))
				{
					return configData[type];
				}
				return null;
			}
		}

		internal void CopyConfigData(ConfigReaderModule other)
		{
			lock (thread_locker)
			{
				if (other == null) return;
				configData.Clear();
				foreach (var p in other.configData)
				{
					configData[p.Key] = p.Value;
				}
			}
		}

		internal void SetConfigWithExample(Type elemType, object example)
		{
			lock (thread_locker)
			{
				configData[elemType] = example;
			}
		}

		public void ResetConfigData()
		{
			lock (thread_locker)
			{
				configData.Clear();
			}
		}

		public abstract IEnumerable<TValue> LoadConfigs<TKey, TValue>() where TValue : class;

		protected object GetConfigCollection(Type elemType)
		{
			lock (thread_locker)
			{
				object collection;
				if (configData.TryGetValue(elemType, out collection))
				{
					return collection;
				}
				return null;
			}
		}

		protected void SetConfigElemByElemType<TKey>(Type elemType, TKey key, object value)
		{
			IDictionary<TKey, object> collection = GetConfigCollection(elemType) as IDictionary<TKey, object>;
			if (collection == null)
			{
				collection = new Dictionary<TKey, object>();
				SetConfigCollection(elemType, collection);
			}
			if (collection != null)
			{
				collection[key] = value;
			}
		}

		protected void SetConfigCollection(Type elemType, object value)
		{
			lock (thread_locker)
			{
				// 不覆盖boot配置
				if (value != null && elemType != null) configData[elemType] = value;
			}
		}

		protected IXmlConfigDictionary CreateXmlDictionaryFromType(Type keyType, object origin = null, bool constConfig = true)
		{
			try
			{
				bool hasOriginDict = origin != null && TypeUtil.IsDictionary(origin.GetType());
				Type generic = typeof(XmlConfigDictionary<>);
				Type xmlDictType = generic.MakeGenericType(keyType);
				var ctor = xmlDictType.GetConstructor(hasOriginDict
					? new[]
					{
						typeof(bool),
						origin.GetType()
					}
					: new Type[]
					{
						typeof(bool)
					});
				if (ctor != null)
				{
					var dict = ctor.Invoke(hasOriginDict
						? new[]
						{
							constConfig,
							origin
						}
						: new object[]
						{
							constConfig
						}) as IDictionary;
					if (dict != null && origin != null && !TypeUtil.IsDictionary(origin.GetType()) && keyType == typeof(int))
					{
						dict[0] = origin;
					}
					return dict as IXmlConfigDictionary;
				}
			}
			catch (Exception e)
			{
				Logger.Warn(e.ToString());
				Logger.Trace(e.ToString());
			}
			return null;
		}

		protected IEnumerable<string> GetFilesFromFolder(string folder, string name, string extension)
		{
			string fieldName = name.ToLower();
			string path = folder + "/" + fieldName;
			string file = path + extension;
			file = PriorityPathManager.Instance.ExternalPathFirst(file);

			if (PlatformManager.Instance.FileExists(file))
			{
				yield return file;
			}
			if (PlatformManager.Instance.DirectoryExists(path))
			{
				foreach (string subfile in PlatformManager.Instance.GetFiles(path, "*" + extension, true))
				{
					if (!subfile.Contains("_history")) yield return subfile;
				}
			}
		}
	}
}
