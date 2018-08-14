using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Kernel.Lang
{
    public interface ISerializableEnum
    {
        int ToInt();
    }

    [Serializable]
    public struct SerializableEnum<T> : IXmlSerializable, ISerializableEnum
    {
        public T Value;

        public SerializableEnum(T value)
        {
            Value = value;
        }

        public static implicit operator SerializableEnum<T>(T v)
        {
            return new SerializableEnum<T>(v);
        }

        public static implicit operator SerializableEnum<T>(int v)
        {
            return new SerializableEnum<T>((T)(object)v);
        }

        public static implicit operator int(SerializableEnum<T> v)
        {
            return v.ToInt();
        }

        public int ToInt()
        {
            return (int)(object)Value;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmptyElement)
            {
                string element = reader.ReadContentAsString();
                if (element != null) Value = (T)Enum.Parse(typeof(T), element);
                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(Value.ToString());
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}