using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kernel.Runtime;
using LitJson;

namespace Kernel.Config
{
	public class BootConfigReaderModule : ConfigReaderModule
	{
		private readonly string bootFolder;
		private string internalBootFolder;

		/// <summary>
		/// bootFolder是可以被自动更新的
		/// internalBootFolder是无法被自动更新的
		/// </summary>
		/// <param name="bootFolder"></param>
		/// <param name="internalBootFolder"></param>
		public BootConfigReaderModule(string bootFolder, string internalBootFolder)
		{
			this.bootFolder = bootFolder;
			this.internalBootFolder = internalBootFolder;
		}

		public override void WriteAllConfig(string targetFolder)
		{

		}

		public override void LoadAllConfig(bool bootOnly = false)
		{
			if (!bootOnly) return;

			ResetConfigData();
			ReadJson(Serializer.Fields.Where(f => f.Boot), (name, type) => GetFilesFromFolder(bootFolder, name, ".json"));
			ReadJson(Serializer.Fields.Where(f => f.Boot), (name, type) => GetFilesFromFolder(internalBootFolder, name, ".json"));
		}

		public override void LoadAllConst()
		{

		}

		public override IWork LoadConfigAutomatically()
		{
			return new AtomWork(() => LoadAllConfig());
		}

		public override IEnumerable<TValue> LoadConfigs<TKey, TValue>()
		{
			var data = GetConfigCollection(typeof(TValue)) as Dictionary<TKey, object>;
			return data != null ? data.Values.Select(o => (TValue)o) : null;
		}

		private void ReadJson(IEnumerable<ConfigFieldInfo> configFields, Func<string, Type, IEnumerable<string>> getFiles)
		{
			var read = typeof(JsonMapper).GetMethod("ReadValue", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(Type), typeof(JsonReader) }, null);
			if (read == null) return;

			foreach (var conf in configFields)
			{
				foreach (var file in getFiles(conf.Name, conf.ConfigType))
				{
					if (conf.FieldMode == ConfigFieldInfo.Mode.CONST)
					{
						SetConfigCollection(conf.ElemType, ReadJsonFromFile(conf.ConfigType, file, read));
					}
					else
					{
						var collection = new Dictionary<int, object>();
						Array array = ReadJsonFromFile(conf.ElemType.MakeArrayType(1), file, read) as Array;
						if (array != null)
						{
							foreach (var obj in array)
							{
								int key = obj.GetValueEx<int>(conf.Key);
								if (key >= 0) collection[key] = obj;
							}
						}
						SetConfigCollection(conf.ElemType, collection);
					}
				}
			}
		}

		private object ReadJsonFromFile(Type configType, string file, MethodInfo read)
		{
			try
			{
				using (TextReader textReader = PlatformManager.Instance.OpenText(file))
				{
					JsonReader reader = new JsonReader(textReader);
					return read.Invoke(null, new object[] { configType, reader });
				}
			}
			catch (Exception e)
			{
				Logger.Fatal("Failed to read config from json {0} : {1}", file, e);
			}
			return null;
		}
	}
}
