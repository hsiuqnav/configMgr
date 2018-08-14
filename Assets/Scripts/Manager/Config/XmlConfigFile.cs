using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kernel.Lang.Collection;

namespace Kernel.Config
{
	public abstract class XmlConfigFile<TValue> where TValue : class
	{
		public bool Changed
		{
			get;
			protected set;
		}

		public string Path
		{
			get;
			protected set;
		}

		public string OriginPath
		{
			get;
			protected set;
		}

		public bool PathUndefined
		{
			get
			{
				return string.IsNullOrEmpty(Path);
			}
		}

		public TValue Config
		{
			get
			{
				return configs.FirstOrDefault();
			}
		}

		public IEnumerable<TValue> Configs
		{
			get
			{
				return configs;
			}
		}

		public int Count
		{
			get
			{
				return configs.Count;
			}
		}

		protected List<TValue> configs;
		protected bool constConfig;

		protected XmlConfigFile(string file, List<TValue> configs, bool constConfig)
		{
			Path = file;
			this.configs = configs;
			this.constConfig = constConfig;
		}

		public void SetConst(TValue conf)
		{
			if (constConfig)
			{
				Remove(0);
				AddAndUpdate(0, conf, configs);
			}
		}

		public void Add(int key, TValue conf)
		{
			AddAndUpdate(key, conf, configs);
		}

		public void Add(string key, TValue conf)
		{
			AddAndUpdate(key, conf, configs);
		}

		public bool Remove(int key)
		{
			return RemoveAndUpdate(key, null, configs);
		}

		public bool Remove(string key)
		{
			return RemoveAndUpdate(key, null, configs);
		}

		public bool Remove(TValue value)
		{
			return RemoveAndUpdate(null, value, configs);
		}

		public bool Contains(TValue value)
		{
			return configs.Contains(value);
		}

		public void SetPath(string p)
		{
			Path = p;
		}

		public void MarkAsChanged()
		{
			Changed = true;
		}

		public void Sort(System.Comparison<TValue> comparer)
		{
			configs.Sort(comparer);
		}

		public virtual void DeleteFile()
		{
			configs.Clear();
		}

		public void RenameFile(string newName)
		{
			if (!newName.EndsWith(".xml")) newName += ".xml";
			if (PathUndefined)
			{
				Path = newName;
			}
			else if (System.IO.Path.IsPathRooted(newName))
			{
				Path = newName;
			}
			else if (Path != null)
			{
				Path = Path.Replace(System.IO.Path.GetFileName(Path), System.IO.Path.GetFileName(newName));
			}
		}

		public virtual void Clear()
		{
			configs.Clear();
		}

		public abstract void DiscardUnsavedChanges();

		public abstract bool Save();

		public bool SaveIfChanged()
		{
			return !Changed || Save();
		}

		protected abstract void AddAndUpdate(object key, TValue conf, List<TValue> confs);

		protected abstract bool RemoveAndUpdate(object key, TValue conf, List<TValue> confs);
	}

	public class XmlConfigFile<TKey, TValue> : XmlConfigFile<TValue> where TValue : class
	{
		internal object Data
		{
			get
			{
				if (constConfig && data != null)
				{
					TValue conf;
					if (data.TryGetValue((TKey)(object)0, out conf))
					{
						return conf;
					}
					return null;
				}
				else
				{
					return data;
				}
			}
		}

		private Dictionary<TKey, TValue> data;
		private static FieldInfo idField;

		static XmlConfigFile()
		{
			var dictAttr = TypeUtil.GetCustomAttribute<DictionaryConfigAttribute>(typeof(TValue), false);
			if (dictAttr != null)
			{
				idField = typeof(TValue).GetField(dictAttr.Key);
			}
		}

		public XmlConfigFile() : this(null)
		{

		}

		public XmlConfigFile(string file, bool constConfig = false) : this(file, new Dictionary<TKey, TValue>(), constConfig)
		{
		}

		public XmlConfigFile(string file, Dictionary<TKey, TValue> configs, bool constConfig) : base(file, configs.Values.ToList(), constConfig)
		{
			data = configs;
			OriginPath = file;
		}

		public override void Clear()
		{
			base.Clear();
			if (data != null) data.Clear();
		}

		public override bool Save()
		{
			UpdateNewIds();

			if (ConfigManager.Instance.SaveXml(this, constConfig))
			{
				Changed = false;
				OriginPath = Path;
				return true;
			}
			return false;
		}

		public override void DeleteFile()
		{
			if (!PathUndefined)
			{
				using (var list = TempList<TValue>.Alloc())
				{
					foreach (var pair in data)
					{
						list.Add(pair.Value);
					}
					foreach (var value in list)
					{
						Remove(value);
					}
				}
				Path = null;
			}
			data.Clear();
			base.DeleteFile();
		}

		public void ChangeId(TKey newKey, TValue conf)
		{
			bool found = false;
			TKey oldKey = default(TKey);
			foreach (var pair in data)
			{
				if (pair.Value == conf)
				{
					found = true;
					oldKey = pair.Key;
					break;
				}
			}
			if (found)
			{
				data.Remove(oldKey);
				data.Add(newKey, conf);
			}
		}

		public override void DiscardUnsavedChanges()
		{
			var module = ConfigManager.Instance.GetModule<ConfigReaderModule>() as XmlConfigReaderModule;
			if (module != null)
			{
				module.ClearConfigsFromFile<TKey, TValue>(OriginPath, constConfig);

				var originConfigs = module.LoadConfigsFromFile<TKey, TValue>(OriginPath, constConfig) as XmlConfigFile<TKey, TValue>;
				if (originConfigs != null)
				{
					configs = originConfigs.configs;
					data = originConfigs.data;
					Changed = false;
					Path = OriginPath;
				}
			}
		}

		protected override void AddAndUpdate(object key, TValue conf, List<TValue> confs)
		{
			RemoveAndUpdate(null, conf, confs);
			data[(TKey)key] = conf;

			confs.Add(conf);

			MarkAsChanged();
		}

		protected override bool RemoveAndUpdate(object key, TValue conf, List<TValue> confs)
		{
			UpdateNewIds();
			bool removed = false;
			if (key != null)
			{
				TKey k = (TKey)key;
				if (data.ContainsKey(k))
				{
					confs.Remove(data[k]);
					data.Remove(k);
					removed = true;
				}
			}
			else if (conf != null)
			{
				bool found = false;
				TKey k = default(TKey);
				foreach (var pair in data)
				{
					if (pair.Value != null && pair.Value.Equals(conf))
					{
						found = true;
						k = pair.Key;
						break;
					}
				}
				if (found)
				{
					confs.Remove(data[k]);
					data.Remove(k);
					removed = true;
				}
			}

			if (removed)
			{
				MarkAsChanged();
			}
			return removed;
		}

		private void UpdateNewIds()
		{
			if (idField != null && data != null)
			{
				var values = data.Values.ToArray();

				data.Clear();
				foreach (var v in values)
				{
					var key = (TKey)idField.GetValue(v);
					data[key] = v;
				}
			}
		}
	}
}