using System;
using System.IO;
#if CODE_GEN || UNITY_EDITOR
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
#endif

namespace Kernel
{
    [Serializable]
#if CODE_GEN || UNITY_EDITOR
    [XmlRoot("timespan")]
    [XmlSchemaProvider("GetSchemaMethod")]
#endif
    public struct SerializableTimeSpan
#if CODE_GEN || UNITY_EDITOR
        : IXmlSerializable
#endif
    {
        private TimeSpan timeSpan;

        public SerializableTimeSpan(long ticks)
        {
            timeSpan = new TimeSpan(ticks);
        }

        public long Ticks
        {
            get
            {
                return timeSpan.Ticks;
            }
        }

        public TimeSpan ToTimeSpan()
        {
            return timeSpan;
        }

        public long Seconds
        {
            get
            {
                return (long)timeSpan.TotalSeconds;
            }
        }

#if CODE_GEN || UNITY_EDITOR
        // XmlType属性和IXmlSerializable接口冲突而无法生效，需要改用添加XmlSchemaProvider接口的方式
        public static XmlQualifiedName GetSchemaMethod(XmlSchemaSet xs)
        {
            string Schema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:import namespace=""http://www.w3.org/2001/XMLSchema"" />
  <xs:element name=""timespan"" nillable=""true"" type=""timespan"" />
  <xs:complexType name=""timespan"" mixed=""true"">
  <xs:sequence>
    <xs:any />
  </xs:sequence>
  </xs:complexType>
</xs:schema>";

            using (var textReader = new StringReader(Schema))
            using (var schemaSetReader = System.Xml.XmlReader.Create(textReader))
            {
                xs.Add("", schemaSetReader);
            }
            return new XmlQualifiedName("timespan");
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
                if (element != null) timeSpan = TimeSpan.Parse(element);
                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(timeSpan.ToString());
        }
#endif
    }
}