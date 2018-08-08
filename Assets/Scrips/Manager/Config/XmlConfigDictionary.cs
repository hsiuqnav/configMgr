using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kernel.Config
{
	public interface IXmlConfigDictionary
	{
		bool IsConstConfig
		{
			get;
		}

		object OriginData
		{
			get;
		}

		bool ContainsFile(string file);
		void ClearFileData(string file);
		void Add(string file, object key, object data);
		void Add(string file, Array data, MemberInfo keyField);
		void DeleteFile(string file);
		bool SaveToFile<TValue>(string file, object configs);
	}

	public interface IXmlConfigDictionary<TKey> : IXmlConfigDictionary
	{
		Dictionary<TKey, TValue> GetFromFile<TValue>(string file);
	}

	public class XmlConfigDictionary<TKey> : Dictionary<TKey, object>, IXmlConfigDictionary<TKey>
	{
		private readonly Dictionary<TKey, string> filePath = new Dictionary<TKey, string>();
		private static Object thread_locker = new Object();

		public bool IsConstConfig
		{
			get;
			private set;
		}

		public object OriginData
		{
			get
			{
				if(IsConstConfig)
					return this[(TKey)(object)0];
				return this;
			}
		}

		public XmlConfigDictionary(bool constConfig)
		{
			IsConstConfig = constConfig;
		}

		public XmlConfigDictionary(bool constConfig, IDictionary<TKey, object> origin) : base(origin)
		{
			IsConstConfig = constConfig;
		}

		public void Add(string file, object key, object value)
		{
			lock(thread_locker)
			{
				filePath[(TKey)key] = file;
				this[(TKey)key] = value;
			}
		}

		public void Add(string file, TKey key, object value)
		{
			lock(thread_locker)
			{
				filePath[key] = file;
				this[key] = value;
			}
		}

		public void Add(string file, Array data, MemberInfo keyField)
		{
			foreach(var item in data)
			{
				TKey key = item.GetValueEx<TKey>(keyField);
				if(key != null) Add(file, key, item);
			}
		}

		public void ClearFileData(string file)
		{
			lock(thread_locker)
			{
				var keys = filePath.Where(p => p.Value.Equals(file, StringComparison.OrdinalIgnoreCase)).Select(p => p.Key).ToArray();
				foreach(var k in keys)
				{
					filePath.Remove(k);
				}
			}
		}

		public void DeleteFile(string file)
		{
			lock(thread_locker)
			{
				List<TKey> deleteKeys = new List<TKey>();
				foreach(var pair in filePath)
				{
					if(file == pair.Value)
					{
						deleteKeys.Add(pair.Key);
					}
				}
				foreach(var key in deleteKeys)
				{
					filePath.Remove(key);
					this.Remove(key);
				}
				System.IO.File.Delete(file);
			}
		}

		public bool ContainsFile(string file)
		{
			lock(thread_locker)
			{
				return filePath.Any(p => p.Value.Equals(file, StringComparison.OrdinalIgnoreCase));
			}
		}

		public Dictionary<TKey, TValue> GetFromFile<TValue>(string file)
		{
			lock(thread_locker)
			{
				return filePath.Where(p => p.Value.Equals(file, StringComparison.OrdinalIgnoreCase))
					.ToDictionary(p => p.Key, p => (TValue)this[p.Key]);
			}
		}

		public bool SaveToFile<TValue>(string file, object configs)
		{
			lock(thread_locker)
			{
				var module = ConfigManager.Instance.GetModule<ConfigReaderModule>() as XmlConfigReaderModule;
				if(module == null || configs == null || !module.WriteXML(file, configs, configs.GetType()))
				{
					return false;
				}

				Dictionary<TKey, TValue> dictConfigs = configs as Dictionary<TKey, TValue>;
				if(dictConfigs == null) return true;

				List<TKey> deleteKeys = new List<TKey>();
				foreach(var pair in filePath)
				{
					if(!dictConfigs.ContainsKey(pair.Key) && file == pair.Value)
					{
						deleteKeys.Add(pair.Key);
					}
				}
				foreach(var key in deleteKeys)
				{
					filePath.Remove(key);
					this.Remove(key);
				}
				foreach(var pair in dictConfigs)
				{
					filePath[pair.Key] = file;
					this[pair.Key] = pair.Value;
				}

				return true;
			}
		}
	}
}
