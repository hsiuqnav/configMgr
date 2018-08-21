using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kernel.Game;
using Kernel.Lang.Collection;
using Kernel.Runtime;
using Kernel.Util;

namespace Kernel.Config
{
	public class BinaryConfigReaderModule : ConfigReaderModule
	{
		private Dictionary<Type, int> fieldIndex = new Dictionary<Type, int>();
		private HashSet<int> isFullLoaded = new HashSet<int>(Int32EqualityComparer.Instance);

		private BinReader reader;
		private ConfigHeader header;
		private int bodyOffset;
		private bool sizeWarning = false;

		protected override IWork DoLoad()
		{
			return new SequenceWork("",
				new AtomWork(OpenAndLoadHeader, "InitConfig"),
				LoadConfigAutomatically());
		}

		public override IWork LoadConfigAutomatically()
		{
			var loaders = TempList<IWork>.Alloc();
			for (int i = 0; i < Serializer.Fields.Length; i++)
			{
				var field = Serializer.Fields[i];

				int index = i;
				DoLoadConfigAutomatically(field, index, loaders);
			}
			if (loaders.Count > 0) return new SequenceWork("", loaders);
			return null;
		}

		public override void WriteAllConfig(string targetFolder)
		{

		}

		public override void LoadAllConfig(bool bootOnly = false)
		{
			ReadAll();
		}

		public override void LoadAllConst()
		{
			ReadAllConst();
		}

		public override T LoadConfig<T>()
		{
			var conf = base.LoadConfig<T>();
			if (conf != null) return conf;

			for (int i = 0; i < Serializer.Fields.Length; i++)
			{
				var field = Serializer.Fields[i];
				if (field.FieldMode == ConfigFieldInfo.Mode.CONST
					&& field.ElemType.IsInstanceOfType(typeof(T)))
					return ReadConst(i, field.ElemType) as T;
			}
			return default(T);
		}

		public override T LoadConfig<T, TKey>(TKey key)
		{
			var conf = base.LoadConfig<T, TKey>(key);
			if (conf != null) return conf;

			var result = ReadElem(typeof(T), key);
			return result as T;
		}

		public override IEnumerable<TValue> LoadConfigs<TKey, TValue>()
		{
			Type elemType = typeof(TValue);

			var fields = Serializer.Fields;
			for (int i = 0; i < fields.Length; i++)
			{
				if (elemType.IsAssignableFrom(fields[i].ElemType))
				{
					ReadAll(i);
					break;
				}
			}

			var dicts = GetConfigCollection(elemType) as IDictionary<TKey, object>;
			if (dicts != null)
			{
				foreach (var pair in dicts)
				{
					yield return (TValue)pair.Value;
				}
			}
		}

		public void OpenAndLoadHeader()
		{
			var binaryConfigPath = PriorityPathManager.Instance.ExternalPathFirst(PathManager.Instance.InternalBinaryConfig);
			reader = new BinReader(PlatformManager.Instance.OpenRead(binaryConfigPath), Encoding.UTF8);
			string md5 = reader.ReadString();
			if (md5 != Serializer.Hash)
			{
				reader.Close();
				reader = null;
				throw new KernelException(KernelResult.BAD_CONFIG_SERIALIZER_MD5, md5, Serializer.Hash);
			}

			header = ReadCurrentPos(typeof(ConfigHeader)) as ConfigHeader;
			bodyOffset = (int)reader.BaseStream.Position;

			for (int iField = 0; iField < Serializer.Fields.Length; iField++)
			{
				var field = Serializer.Fields[iField];
				fieldIndex[field.ElemType] = iField;
			}

			ConfigManager.Instance.LoadAllConst();
			Logger.Trace("OpenAndLoadHeader");
		}

		protected override void OnUnload()
		{
			if (reader != null) reader.Close();
		}

		protected void DoLoadConfigAutomatically(ConfigFieldInfo field, int index, TempList<IWork> loaders)
		{
			if (field.LoadAll)
				loaders.Add(new AtomWork(() => ReadAll(index)));
			else if (field.FieldMode == ConfigFieldInfo.Mode.CONST)
				loaders.Add(new AtomWork(() => ReadConst(index, field.ElemType)));
		}

		private object ReadCurrentPos(Type type)
		{
			return Serializer.Serializers[type].Read(reader, null);
		}

		public void ReadAll()
		{
			for (int i = 0; i < Serializer.Fields.Length; i++)
			{
				ReadAll(i);
			}
		}

		public void ReadAllConst()
		{
			for (int i = 0; i < Serializer.Fields.Length; i++)
			{
				var field = Serializer.Fields[i];
				if (field.FieldMode == ConfigFieldInfo.Mode.CONST)
					ReadAll(i);
			}
		}

		protected void ReadAll(int index)
		{
			if (isFullLoaded.Contains(index))
				return;

			var field = Serializer.Fields[index];
			if (field.FieldMode == ConfigFieldInfo.Mode.CONST)
			{
				ReadConst(index, field.ElemType);
			}
			else
			{
				WarningIfReadHuge(index);

				Dictionary<int, int> offsetInt = header.Contents[index].OffsetDict as Dictionary<int, int>;
				if (offsetInt != null)
				{
					foreach (var elem in offsetInt)
					{
						ReadElem(field.ElemType, elem.Key, elem.Value);
					}
					isFullLoaded.Add(index);
				}
				Dictionary<string, int> offsetString = header.Contents[index].OffsetDict as Dictionary<string, int>;
				if (offsetString != null)
				{
					foreach (var elem in offsetString)
					{
						ReadElem(field.ElemType, elem.Key, elem.Value);
					}
					isFullLoaded.Add(index);
				}
			}
		}

		private object ReadConst(int index, Type elemType)
		{
			reader.BaseStream.Seek(header.Contents[index].Offset + bodyOffset, SeekOrigin.Begin);
			var value = ReadCurrentPos(elemType);

			SetConfigCollection(elemType, value);
			return value;
		}

		private void WarningIfReadHuge(int index)
		{
			if (!sizeWarning)
				return;

			var field = header.Contents[index];
			if (field.Size > 50 * 1024)
				Logger.Warn("游戏过程中动态加载了{0}全表文件{1}，内存占用约 *2 ~ *3",
					StringUtil.BytesFormatString(field.Size), Serializer.Fields[index].Name);
		}

		private object ReadElem<TKey>(Type elemType, TKey key)
		{
			int index;
			if (fieldIndex.TryGetValue(elemType, out index))
			{
				Dictionary<TKey, int> elemOffset = header.Contents[index].OffsetDict as Dictionary<TKey, int>;
				if (elemOffset != null && elemOffset.ContainsKey(key))
					return ReadElem<TKey>(elemType, key, elemOffset[key]);
			}
			return null;
		}

		private object ReadElem<TKey>(Type elemType, TKey key, int offset)
		{
			reader.BaseStream.Seek(offset + bodyOffset, SeekOrigin.Begin);
			var elem = ReadCurrentPos(elemType);

			SetConfigElemByElemType<TKey>(elemType, key, elem);
			return elem;
		}
	}
}
