using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Kernel
{
	[Serializable]
	public class SerializableTwoDimArray<T> : IXmlSerializable
	{
		private static XmlSerializer arraySerializer;
		private static XmlSerializer valueSerializer;

		private IList<T>[] array;

		public T this[int first, int second]
		{
			get
			{
				if (first >= 0 && first < array.Length)
				{
					var list = array[first];
					if (list != null && second >= 0 && second < list.Count)
					{
						return list[second];
					}
				}
				return default(T);
			}
			set
			{
				if (first >= 0 && first < array.Length)
				{
					var list = array[first];
					if (list != null && second >= 0 && second < list.Count)
					{
						list[second] = value;
					}
				}
			}
		}

		public IList<T> this[int first]
		{
			get
			{
				if (first >= 0 && first < array.Length)
				{
					return array[first];
				}
				return null;
			}
			set
			{
				if (first >= 0 && first < array.Length)
				{
					array[first] = value;
				}
			}
		}

		public int Length
		{
			get
			{
				return array != null ? array.Length : 0;
			}
		}

#if CODE_GEN || UNITY_EDITOR
		public SerializableTwoDimArray() : this(2)
		{
			array[0] = new T[2];
			array[1] = new T[3];
		}
#endif

		public SerializableTwoDimArray(int firstDimensionSize)
		{
			array = new IList<T>[firstDimensionSize];
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			if (reader.IsEmptyElement)
			{
				reader.Read();
			}
			else
			{
				List<T[]> collection = new List<T[]>();
				reader.Read();
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					reader.MoveToContent();
					T[] value = (T[])ArraySerializer.Deserialize(reader);
					if (value != null) collection.Add(value);
				}
				reader.ReadEndElement();
				array = collection.Select(r => r as IList<T>).ToArray();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach (IList<T> list in array)
			{
				if (list != null)
				{
					ArraySerializer.Serialize(writer, list, XmlUtil.DefaultNamespace);
				}
			}
		}

		private static XmlSerializer ArraySerializer
		{
			get
			{
				if (arraySerializer == null)
					arraySerializer = new XmlSerializer(typeof(T[]), new XmlRootAttribute(TypeUtil.GetSerializeTypeName(typeof(T[]), true)));
				return arraySerializer;
			}
		}
	}
}
