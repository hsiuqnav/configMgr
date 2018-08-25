using System;
using System.IO;

namespace Kernel.Config
{
    public class SerializerListVisitor
    {
        public void Export(SerializationGenerator generator, string nameSpace, string md5, string targetFolder)
        {
            var builder = new TextBuilder();

            builder.WriteLine("using System;");
            builder.WriteLine("using System.Collections.Generic;");
            builder.WriteLine("using System.IO;");
            builder.WriteLine("using Kernel;");
            builder.WriteLine("using Kernel.Config;");
            builder.EmptyLine();

            builder.WriteLine("namespace {0}", nameSpace);
            builder.LeftPar();
            builder.Indent();

            builder.WriteLine("public partial class ConfigSerializer : ConfigSerializerBase");
            builder.LeftPar();
            builder.Indent();

            builder.WriteLine("private static readonly ConfigFieldInfo[] fields =");
            builder.WriteLine("{");
            builder.Indent();

            foreach (var pair in generator.RegisteredTypes)
            {
                if (pair.IsEntry && !pair.Root.PoliClass)
                {
                    VisitEntry(pair, builder);
                }
            }

            builder.UnIndent();
            builder.WriteLine("};");

            builder.EmptyLine();

            builder.WriteLine("public override ConfigFieldInfo[] Fields { get { return fields; } }");

            builder.EmptyLine();

            builder.WriteLine("public override string Hash {{ get {{ return \"{0}\"; }} }}", md5);

            builder.EmptyLine();

            builder.WriteLine("private static readonly Dictionary<Type, IConfigSerializer> serializers = new Dictionary<Type, IConfigSerializer>");
            builder.WriteLine("{");
            builder.Indent();

            foreach (var pair in generator.RegisteredTypes)
            {
                if ((pair.Root.PoliClass || !XmlUtil.IsPolymorphicClass(pair.Type)) && !pair.IsEnum)
                {
                    VisitSerializer(pair, builder);
                }
            }

            builder.UnIndent();
            builder.WriteLine("};");

            builder.EmptyLine();

            builder.WriteLine("public override Dictionary<Type, IConfigSerializer> Serializers { get { return serializers; } }");

            builder.UnIndent();
            builder.RightPar();

            builder.UnIndent();
            builder.RightPar();

            builder.WriteToFile(Path.Combine(targetFolder, "ConfigSerializer.cs"));
        }

        public void VisitEntry(RootNode node, TextBuilder builder)
        {
            var elemType = node.ElemType.Type;
            ConfigAttribute attribute = TypeUtil.GetAttribute<ConfigAttribute>(elemType);
            if (attribute != null && !elemType.IsEnum)
            {
                Type configType = attribute.GetConfigType(elemType);
                DictionaryConfigAttribute dictAttr = attribute as DictionaryConfigAttribute;
                WriteFieldEntry(builder, configType, attribute.Name ?? ConfigManager.Instance.GetDefaultConfigFileName(configType),
                    dictAttr != null ? dictAttr.Key : null,
                    dictAttr != null && dictAttr.LoadAll);
            }
        }

        public void VisitSerializer(RootNode node, TextBuilder builder)
        {
            builder.WriteLine("{{ typeof({0}), new {1}() }},",
                TypeUtil.GetGenericTypeNameWithParams(node.Type, true),
                node.Header.SerializerTypeName);
        }

        private static void WriteFieldEntry(TextBuilder builder, Type configType, string name, string key, bool loadAll)
        {
            bool isDict = TypeUtil.IsDictionary(configType);

            Type elemType = isDict ? TypeUtil.GetDictionaryValueType(configType) : configType;
            builder.WriteLine("new ConfigFieldInfo(\"{0}\", typeof({1}), typeof({2}), ConfigFieldInfo.Mode.{3}, {4}, \"{5}\"),",
                name,
                TypeUtil.GetCSharpFullTypeName(configType),
                TypeUtil.GetCSharpFullTypeName(elemType),
                isDict ? "KEY_VALUE" : "CONST",
                loadAll.ToString().ToLower(),
                key);
        }
    }
}
