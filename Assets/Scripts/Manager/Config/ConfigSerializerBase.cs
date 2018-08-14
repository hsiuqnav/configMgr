using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Kernel.Config
{
	public abstract class ConfigSerializerBase
	{
		public virtual ConfigFieldInfo[] Fields
		{
			get
			{
				return null;
			}
		}

		public virtual Dictionary<Type, IConfigSerializer> Serializers
		{
			get
			{
				return null;
			}
		}

		public virtual string Hash
		{
			get
			{
				return null;
			}
		}

		public virtual string LuaConfigTemplate
		{
			get
			{
				return null;
			}
		}

		public virtual bool IsLuaSerializer
		{
			get
			{
				return false;
			}
		}

		public void WriteToBinary(BinWriter writer)
		{
			using (MemoryStream bodyStream = new MemoryStream())
			using (BinWriter bodyWriter = new BinWriter(bodyStream))
			{
				writer.Write(Hash);

				if (Fields == null) return;

				ConfigHeader header = new ConfigHeader();
				header.Contents = new FieldLayout[Fields.Length];

				for (int i = 0; i < Fields.Length; i++)
				{
					var f = Fields[i];

					int offset = (int)bodyWriter.BaseStream.Position;

					var elemSerializer = Serializers[f.ElemType];
					if (f.FieldMode == ConfigFieldInfo.Mode.CONST)
					{
						header.Contents[i] = FieldLayout.CreateFieldLayout(f.FieldMode, offset, null);
						object obj = ConfigManager.Instance.GetConfig(f.ElemType);
						elemSerializer.Write(bodyWriter, obj);
					}
					else
					{
						var dict = (IDictionary)ConfigManager.Instance.GetConfig(f.ElemType);
						if (dict == null) dict = TypeUtil.CreateInstance(f.ConfigType) as IDictionary;

						var layout = header.Contents[i] = FieldLayout.CreateFieldLayout(f.FieldMode, offset, TypeUtil.GetDictionaryKeyType(dict.GetType()));

						var e = dict.GetEnumerator();
						while (e.MoveNext())
						{
							layout.Add(e.Key, (int)bodyWriter.BaseStream.Position);
							elemSerializer.Write(bodyWriter, e.Value);
						}

						layout.Size = (int)bodyWriter.BaseStream.Position - offset;
					}
				}

				using (var headerStream = new MemoryStream())
				using (var headerWriter = new BinWriter(headerStream))
				{
					Serializers[typeof(ConfigHeader)].Write(headerWriter, header);

					header.HeaderSize = (int)headerWriter.BaseStream.Position;
					header.BodySize = (int)bodyWriter.BaseStream.Position;

					Serializers[typeof(ConfigHeader)].Write(writer, header);

					writer.Write(bodyStream.GetBuffer());
				}
			}
		}
	}
}