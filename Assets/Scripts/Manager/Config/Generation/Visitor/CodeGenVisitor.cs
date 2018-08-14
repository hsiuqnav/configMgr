using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kernel.Config
{
    public class CodeGenVisitor
    {
        private readonly Dictionary<Type, string> primitiveTypeString = new Dictionary<Type, string>
        {
            { typeof(bool), "Boolean" },
            { typeof(byte), "Byte" },
            { typeof(sbyte), "SByte" },
            { typeof(short), "Int16" },
            { typeof(ushort), "UInt16" },
            { typeof(int), "Int32" },
            { typeof(uint), "UInt32" },
            { typeof(long), "Int64" },
            { typeof(ulong), "UInt64" },
            { typeof(float), "Single" },
            { typeof(double), "Double" },
        };

        private const string VARIABLE = "d";
        private const string STREAM = "o";

        public void Export(SerializationGenerator generator, string nameSpace, string targetFolder)
        {
            foreach (var pair in generator.RegisteredTypes)
            {
                if (pair.IsEnum) continue;

                var builder = new TextBuilder();
                VisitRoot(pair, nameSpace, builder);
                pair.CodeMd5 = MD5Util.MD5(builder.ToString());

                builder.WriteToFile(Path.Combine(targetFolder, pair.FileName + ".cs"));
            }
        }

        public void VisitRoot(RootNode node, string nameSpace, TextBuilder builder)
        {
            VisitHeader(node.Header, nameSpace, builder);

            VisitReadPart(node.ReadPart, builder);

            builder.EmptyLine();

            VisitWritePart(node.WritePart, builder);

            FinishVisitHeader(node.Header, builder);
        }

        protected virtual void VisitHeader(TypeNode node, string nameSpace, TextBuilder builder)
        {
            builder.WriteLine("// Auto generated code");
            builder.WriteLine("using Kernel;");
            builder.WriteLine("using Kernel.Config;");
            builder.WriteLine("using Kernel.Engine;");
            builder.WriteLine("using Kernel.Util;");
            builder.WriteLine("using System;");
            builder.WriteLine("using System.IO;");
            builder.EmptyLine();

            builder.WriteLine("namespace {0}", nameSpace);
            builder.LeftPar();
            builder.Indent();
            builder.WriteLine("public class {0} : IConfigSerializer", node.SerializerTypeName);
            builder.LeftPar();
            builder.Indent();
        }

        public void FinishVisitHeader(TypeNode node, TextBuilder builder)
        {
            builder.UnIndent();
            builder.RightPar();
            builder.UnIndent();
            builder.RightPar();
        }

        protected virtual void VisitReadPart(ReadPartNode node, TextBuilder builder)
        {
            builder.WriteLine("object IConfigSerializer.Read(IBinaryReader reader, object o)");
            builder.LeftPar();
            builder.Indent();
            builder.WriteLine("return Read(reader, ({0})o);", node.TypeName);
            builder.UnIndent();
            builder.RightPar();

            builder.EmptyLine();

            builder.WriteLine("public static {0} Read(IBinaryReader o, {0} d)", node.TypeName);
            builder.LeftPar();
            builder.Indent();

            if (!node.Type.IsValueType)
            {
                builder.WriteLine("if(o.ReadBoolean() == false) return null;");
                builder.EmptyLine();
            }

            var root = node.Parent as RootNode;
            if (root != null && root.CustomSerializeParams != null)
            {
                AppendReadFromCustom("o", root.CustomSerializeParams, builder);
            }
            else
            {
                AppendReadPart(node, builder);
            }

            builder.WriteLine("return d;");

            builder.UnIndent();
            builder.RightPar();
        }

        protected void AppendReadFromCustom(string reader, SerializeNode[] customParams, TextBuilder builder)
        {
            string parameters = "", sep = "";
            for (int i = 0; i < customParams.Length; i++)
            {
                var p = customParams[i];

                builder.WriteLine("var data{0} = default({1});", i, p.TypeName); ;
                VisitReadField("data" + i, reader, false, p, builder);
                parameters += sep + "data" + i;
                sep = ", ";
            }
            builder.WriteLine("d.FromSerializedData({1}, {0});", parameters, reader);
        }

        protected void AppendReadPart(ReadPartNode node, TextBuilder builder)
        {
            if (node.FirstChild is ArrayTypeNode)
            {
                VisitReadArray(VARIABLE, STREAM, node.FirstChild as ArrayTypeNode, builder);
            }
            else if (node.FirstChild is ListTypeNode)
            {
                VisitReadList(VARIABLE, STREAM, node.FirstChild as ListTypeNode, builder);
            }
            else if (node.FirstChild is DictionaryTypeNode)
            {
                VisitReadDictionary(VARIABLE, STREAM, node.FirstChild as DictionaryTypeNode, builder);
            }
            else if (node.FirstChild is HashSetTypeNode)
            {
                VisitReadHashSet(VARIABLE, STREAM, node.FirstChild as HashSetTypeNode, builder);
            }
            else if (node.FirstChild is PolyClassTypeNode)
            {
                VisitReadPolyClass(VARIABLE, STREAM, node.FirstChild as PolyClassTypeNode, builder);
            }
            else if (node.FirstChild is TwoDimArrayTypeNode)
            {
                VisitReadTwoDimArray(VARIABLE, STREAM, node.FirstChild as TwoDimArrayTypeNode, builder);
            }
            else if (node.FirstChild is StringTypeNode)
            {
                VisitReadString(VARIABLE, STREAM, builder);
            }
            else
            {
                AppendReadAllFields(node, builder);
            }
        }

        protected virtual void AppendReadAllFields(ReadPartNode node, TextBuilder builder)
        {
            if (node.CreateNew) builder.WriteLine("if(d == null) d = new {0}();", node.TypeName);

            foreach (var child in node.Children)
            {
                if (child is FieldNode)
                {
                    var field = child as FieldNode;
                    builder.Push();
                    VisitField("d", field, builder);
                    var variable = builder.Pop();

                    VisitReadField(variable, "o", field.ReadOnly, field.Serialize, builder);
                }
            }
        }

        public void VisitReadField(string variable, string reader, bool readOnly, SerializeNode node, TextBuilder builder)
        {
            if (node == null) return;

            builder.WriteIndents();
            if (node.Category == SerializeNode.NodeCategory.LOCALE)
            {
                builder.Write("{0}.Set(StringSerializer.Read({1}, null));", variable, reader);
            }
            else
            {
                if (!readOnly)
                {
                    builder.Write(variable);
                    builder.Write(" = ");
                }
                VisitDeserialization(variable, reader, node, builder);
                if (!readOnly && node.Category == SerializeNode.NodeCategory.CLASS)
                {
                    builder.Write(" as {0}", node.TypeName);
                }
                builder.Write(";");
            }
            builder.NextLine();
        }

        public void VisitReadString(string variable, string reader, TextBuilder builder)
        {
            builder.WriteLine("{0} = {1}.ReadString();", variable, reader);
        }

        public void VisitReadArray(string variable, string reader, ArrayTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("int size = {0}.ReadInt32();", reader);
            builder.WriteLine("if({0} == null || {0}.Length != size) {0} = new {1}[size];", variable, node.ParameterNodes[0].TypeName);
            builder.WriteLine("for(int i = 0; i < size; ++i)");
            builder.LeftPar();
            builder.Indent();

            string target = variable + "[i]";
            VisitReadField(target, reader, false, node.Serialize, builder);

            builder.UnIndent();
            builder.RightPar();
        }

        public void VisitReadTwoDimArray(string variable, string reader, TwoDimArrayTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("int size0 = {0}.ReadInt32();", reader);
            builder.WriteLine("{0} = new {1}(size0);", variable, node.TypeName);
            builder.WriteLine("for(int i = 0; i < size0; ++i)");
            builder.LeftPar();
            builder.Indent();

            builder.WriteLine("int size1 = {0}.ReadInt32();", reader);
            builder.WriteLine("{0}[i] = new {1}[size1];", variable, node.ParameterNodes[0].TypeName);
            builder.WriteLine("for(int j = 0; j < size1; ++j)");
            builder.LeftPar();
            builder.Indent();

            string target = variable + "[i, j]";
            builder.WriteIndents();
            builder.Write(target + " = ");
            VisitDeserialization(target, reader, node.Serialize, builder);
            builder.Write(";");
            builder.NextLine();

            builder.UnIndent();
            builder.RightPar();

            builder.UnIndent();
            builder.RightPar();
        }

        public void VisitReadList(string variable, string reader, ListTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("int size = {0}.ReadInt32();", reader);
            builder.WriteLine("if({0} == null) ", variable);
            builder.WriteLine("\t{0} = new {1}(size);", variable, node.TypeName);
            builder.WriteLine("else if({0}.Count != size)", variable);
            builder.WriteLine("\t{0}.Clear();", variable);
            builder.WriteLine("for(int i = 0; i < size; ++i)");
            builder.LeftPar();
            builder.Indent();

            builder.WriteIndents();
            builder.Write("var temp = ", variable);
            VisitDeserialization(string.Format("(d.Count > i ? d[i] : default({0}))", node.ParameterNodes[0].TypeName), reader, node.Serialize, builder);
            builder.Write(";");
            builder.NextLine();

            builder.WriteLine("if(d.Count <= i) d.Add(temp); else d[i] = temp;");

            builder.UnIndent();
            builder.RightPar();
        }

        public void VisitReadHashSet(string variable, string reader, HashSetTypeNode node, TextBuilder builder)
        {
            string equalityComparer = GetEqualityComparer(node.Serialize.Type);

            builder.WriteLine("int size = {0}.ReadInt32();", reader);
            builder.WriteLine("{0} = new {1}({2});", variable, node.TypeName, equalityComparer);
            builder.WriteLine("for(int i = 0; i < size; ++i)");
            builder.LeftPar();
            builder.Indent();

            builder.WriteIndents();
            builder.Write("{0}.Add(", variable);
            VisitDeserialization(string.Format("default({0})", node.ParameterNodes[0].TypeName), reader, node.Serialize, builder);
            builder.Write(");");
            builder.NextLine();

            builder.UnIndent();
            builder.RightPar();
        }

        public void VisitReadDictionary(string variable, string reader, DictionaryTypeNode node, TextBuilder builder)
        {
            string equalityComparer = GetEqualityComparer(node.SerializeKey.Type);

            builder.WriteLine("int size = {0}.ReadInt32();", reader);
            builder.WriteLine("if({0} == null) ", variable);
            builder.WriteLine("\t{0} = new {1}(size{2});", variable, node.TypeName,
                string.IsNullOrEmpty(equalityComparer) ? "" : ", " + equalityComparer);
            builder.WriteLine("else");
            builder.WriteLine("\t{0}.Clear();", variable);
            builder.WriteLine("for(int i = 0; i < size; ++i)");
            builder.LeftPar();
            builder.Indent();

            builder.WriteIndents();
            builder.Write("{0} key = ", node.ParameterNodes[0].TypeName);
            VisitDeserialization(string.Format("default({0})", node.ParameterNodes[0].TypeName), reader, node.SerializeKey, builder);
            builder.Write(";");
            builder.NextLine();

            builder.WriteIndents();
            builder.Write("{0} value = ", node.ParameterNodes[1].TypeName);
            VisitDeserialization(string.Format("default({0})", node.ParameterNodes[1].TypeName), reader, node.SerializeValue, builder);
            builder.Write(";");
            builder.NextLine();

            builder.WriteLine("{0}.Add(key, value);", variable);

            builder.UnIndent();
            builder.RightPar();
        }

        protected virtual string GetEqualityComparer(Type keyType)
        {
            string equalityComparer = null;
            if (keyType.IsEnum || keyType.IsPrimitive)
            {
                equalityComparer = keyType.Name + "EqualityComparer.Instance";

                if (keyType.Namespace != null && !keyType.Namespace.StartsWith("System"))
                {
                    equalityComparer = keyType.Namespace + "." + equalityComparer;
                }
            }
            return equalityComparer;
        }

        protected void VisitReadPolyClass(string variable, string reader, PolyClassTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("int type = {0}.ReadInt32();", reader);
            builder.WriteLine("switch(type)");
            builder.LeftPar();
            builder.Indent();

            var children = node.Children.ToArray();
            for (int index = 0; index < children.Length; index++)
            {
                var n = children[index] as SerializeNode;

                builder.WriteLine("case {0}:", index + 1);

                builder.Indent();
                builder.WriteIndents();
                builder.Write("{0} = ", variable);
                VisitDeserialization(variable, reader, n, builder);
                builder.Write(";");
                builder.NextLine();

                builder.WriteLine("break;");
                builder.UnIndent();
            }

            builder.UnIndent();
            builder.RightPar();

        }

        public void VisitDeserialization(string variable, string reader, SerializeNode node, TextBuilder builder)
        {
            switch (node.Category)
            {
                case SerializeNode.NodeCategory.PRIMITIVE:
                    VisitPrimitiveDeserialization(reader, node, builder);
                    break;
                case SerializeNode.NodeCategory.ENUM:
                    VisitEnumDeserialization(reader, node, builder);
                    break;
                case SerializeNode.NodeCategory.FIXED:
                    VisitFixedDeserialization(reader, node, builder);
                    break;
                case SerializeNode.NodeCategory.TIMESPAN:
                    VisitTimeSpanDeserialization(reader, node, builder);
                    break;
                case SerializeNode.NodeCategory.DATETIME:
                    VisitDateTimeDeserialization(reader, node, builder);
                    break;
                case SerializeNode.NodeCategory.COLOR:
                    VisitColorDeserialization(reader, node, builder);
                    break;
                case SerializeNode.NodeCategory.POLY_CLASS:
                case SerializeNode.NodeCategory.CLASS:
                case SerializeNode.NodeCategory.ARRAY:
                case SerializeNode.NodeCategory.LIST:
                case SerializeNode.NodeCategory.DICTIONARY:
                    VisitClassDeserialization(variable, reader, node, builder);
                    break;
                case SerializeNode.NodeCategory.STRUCT:
                    VisitStructDeserialization(variable, reader, node, builder);
                    break;
                case SerializeNode.NodeCategory.NULLABLE:
                    VisitNullableDeserialization(variable, reader, node as NullableSerializationNode, builder);
                    break;
            }
        }

        public void VisitPrimitiveDeserialization(string reader, SerializeNode node, TextBuilder builder)
        {
            string str;
            if (primitiveTypeString.TryGetValue(node.Type, out str))
            {
                builder.Write("{0}.Read{1}()", reader, str);
            }
        }

        public void VisitEnumDeserialization(string reader, SerializeNode node, TextBuilder builder)
        {
            builder.Write("({0}){1}.ReadInt32()", node.TypeName, reader);
        }

        public virtual void VisitFixedDeserialization(string reader, SerializeNode node, TextBuilder builder)
        {
            builder.Write("{0}.ReadFixed()", reader);
        }

        public void VisitTimeSpanDeserialization(string reader, SerializeNode node, TextBuilder builder)
        {
            builder.Write("new SerializableTimeSpan({0}.ReadInt64() * TimeSpan.TicksPerSecond)", reader);
        }

        public void VisitDateTimeDeserialization(string reader, SerializeNode node, TextBuilder builder)
        {
            builder.Write("TimeUtil.CTimeToUtcDate({0}.ReadInt64())", reader);
        }

        public void VisitColorDeserialization(string reader, SerializeNode node, TextBuilder builder)
        {
            builder.Write("(Kernel.Engine.Color){0}.ReadUInt32()", reader);
        }

        public void VisitStructDeserialization(string variable, string reader, SerializeNode node, TextBuilder builder)
        {
            builder.Write("{0}.Read({1}, {2})", node.TypeNode.SerializerTypeName, reader, variable);
        }

        protected virtual void VisitClassDeserialization(string variable, string reader, SerializeNode node, TextBuilder builder)
        {
            builder.Write("{0}.Read({1}, {3} as {2})", node.TypeNode.SerializerTypeName, reader, node.TypeName, variable);
        }

        protected virtual void VisitNullableDeserialization(string variable, string reader, NullableSerializationNode node, TextBuilder builder)
        {
            builder.Write("!{0}.ReadBoolean() ? null : new {1}(", reader, node.TypeName);
            VisitDeserialization(string.Format("default({0})", node.Serialize.TypeName), reader, node.Serialize, builder);
            builder.Write(")");
        }

        protected virtual void VisitWritePart(WritePartNode node, TextBuilder builder)
        {
            builder.WriteLine("void IConfigSerializer.Write(IBinaryWriter writer, object o)");
            builder.LeftPar();
            builder.Indent();
            builder.WriteLine("Write(writer, ({0})o);", node.TypeName);
            builder.UnIndent();
            builder.RightPar();

            builder.EmptyLine();

            builder.WriteLine("public static void Write(IBinaryWriter o, {0} d)", node.TypeName);
            builder.LeftPar();
            builder.Indent();

            if (!node.Type.IsValueType)
            {
                builder.WriteLine("o.Write(d != null);");
                builder.WriteLine("if(d == null) return;");
                builder.EmptyLine();
            }

            var root = node.Parent as RootNode;
            if (root != null && root.CustomSerializeParams != null)
            {
                AppendWriteFromCustom("o", root.CustomSerializeParams, builder);
            }
            else
            {
                AppendWritePart(node, builder);
            }

            builder.UnIndent();
            builder.RightPar();
        }

        protected void AppendWriteFromCustom(string writer, SerializeNode[] customParams, TextBuilder builder)
        {
            string parameters = "", sep = "";
            for (int i = 0; i < customParams.Length; i++)
            {
                var p = customParams[i];
                builder.WriteLine("{1} data{0};", i, p.TypeName);
                parameters += sep + "out data" + i;
                sep = ", ";
            }
            builder.WriteLine("d.ToSerializedData({1}, {0});", parameters, writer);
            for (int i = 0; i < customParams.Length; i++)
            {
                var p = customParams[i];
                VisitWriteField("data" + i, writer, p, builder);
            }
        }

        protected void AppendWritePart(WritePartNode node, TextBuilder builder)
        {
            if (node.FirstChild is ArrayTypeNode)
            {
                VisitWriteArray(VARIABLE, STREAM, node.FirstChild as ArrayTypeNode, builder);
            }
            else if (node.FirstChild is ListTypeNode)
            {
                VisitWriteList(VARIABLE, STREAM, node.FirstChild as ListTypeNode, builder);
            }
            else if (node.FirstChild is DictionaryTypeNode)
            {
                VisitWriteDictionary(VARIABLE, STREAM, node.FirstChild as DictionaryTypeNode, builder);
            }
            else if (node.FirstChild is HashSetTypeNode)
            {
                VisitWriteHashSet(VARIABLE, STREAM, node.FirstChild as HashSetTypeNode, builder);
            }
            else if (node.FirstChild is PolyClassTypeNode)
            {
                VisitWritePolyClass(VARIABLE, STREAM, node.FirstChild as PolyClassTypeNode, builder);
            }
            else if (node.FirstChild is TwoDimArrayTypeNode)
            {
                VisitWriteTwoDimArray(VARIABLE, STREAM, node.FirstChild as TwoDimArrayTypeNode, builder);
            }
            else if (node.FirstChild is StringTypeNode)
            {
                VisitWriteString(VARIABLE, STREAM, builder);
            }
            else
            {
                foreach (var child in node.Children)
                {
                    if (child is FieldNode)
                    {
                        var field = child as FieldNode;
                        builder.Push();
                        VisitField(VARIABLE, field, builder);
                        var variable = builder.Pop();

                        VisitWriteField(variable, STREAM, field.Serialize, builder);
                    }
                }
            }
        }

        public void VisitWriteField(string variable, string writer, SerializeNode node, TextBuilder builder)
        {
            if (node == null) return;

            builder.WriteIndents();
            if (node.Category == SerializeNode.NodeCategory.LOCALE)
            {
                builder.Write("StringSerializer.Write({0}, {1}.ToString());", writer, variable);
            }
            else
            {
                VisitSerialization(variable, writer, node, builder);
                builder.Write(";");
            }
            builder.NextLine();
        }

        public void VisitWriteString(string variable, string writer, TextBuilder builder)
        {
            builder.WriteLine("{0}.Write({1});", writer, variable);
        }

        public void VisitWriteArray(string variable, string writer, ArrayTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("int size = {0}.Length;", variable);
            builder.WriteLine("{0}.Write(size);", writer);
            builder.WriteLine("for(int i = 0; i < size; ++i)");
            builder.LeftPar();
            builder.Indent();

            VisitWriteField(variable + "[i]", writer, node.Serialize, builder);

            builder.UnIndent();
            builder.RightPar();
        }

        public void VisitWriteTwoDimArray(string variable, string writer, TwoDimArrayTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("int size0 = {0}.Length;", variable);
            builder.WriteLine("{0}.Write(size0);", writer);
            builder.WriteLine("for(int i = 0; i < size0; ++i)");
            builder.LeftPar();
            builder.Indent();

            builder.WriteLine("int size1 = {0}[i] != null ? {0}[i].Count : 0;", variable);
            builder.WriteLine("{0}.Write(size1);", writer);
            builder.WriteLine("for(int j = 0; j < size1; ++j)");
            builder.LeftPar();
            builder.Indent();

            builder.WriteIndents();
            VisitSerialization(variable + "[i, j]", writer, node.Serialize, builder);
            builder.Write(";");
            builder.NextLine();

            builder.UnIndent();
            builder.RightPar();
            builder.UnIndent();
            builder.RightPar();
        }

        public void VisitWriteList(string variable, string writer, ListTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("int size = {0}.Count;", variable);
            builder.WriteLine("{0}.Write(size);", writer);
            builder.WriteLine("for(int i = 0; i < size; ++i)");
            builder.LeftPar();
            builder.Indent();

            builder.WriteIndents();
            VisitSerialization(variable + "[i]", writer, node.Serialize, builder);
            builder.Write(";");
            builder.NextLine();

            builder.UnIndent();
            builder.RightPar();
        }

        public void VisitWriteHashSet(string variable, string writer, HashSetTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("int size = {0}.Count;", variable);
            builder.WriteLine("{0}.Write(size);", writer);
            builder.WriteLine("foreach(var elem in {0})", variable);
            builder.LeftPar();
            builder.Indent();

            builder.WriteIndents();
            VisitSerialization("elem", writer, node.Serialize, builder);
            builder.Write(";");
            builder.NextLine();

            builder.UnIndent();
            builder.RightPar();
        }

        public void VisitWriteDictionary(string variable, string writer, DictionaryTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("int size = {0}.Count;", variable);
            builder.WriteLine("{0}.Write(size);", writer);
            builder.WriteLine("foreach(var elem in {0})", variable);
            builder.LeftPar();
            builder.Indent();

            builder.WriteIndents();
            VisitSerialization("elem.Key", writer, node.SerializeKey, builder);
            builder.Write(";");
            builder.NextLine();

            builder.WriteIndents();
            VisitSerialization("elem.Value", writer, node.SerializeValue, builder);
            builder.Write(";");
            builder.NextLine();

            builder.UnIndent();
            builder.RightPar();
        }

        public void VisitWritePolyClass(string variable, string writer, PolyClassTypeNode node, TextBuilder builder)
        {
            builder.WriteLine("Type type = {0}.GetType();", variable);

            var children = node.Children.ToArray();

            for (int index = 0; index < children.Length; index++)
            {
                var n = children[index] as SerializeNode;

                builder.WriteIndents();
                if (index > 0) builder.Write("else ");
                builder.Write("if(type == typeof({0}))", n.TypeName);
                builder.NextLine();

                builder.LeftPar();
                builder.Indent();

                builder.WriteLine("{0}.Write({1});", writer, index + 1);

                builder.WriteIndents();
                VisitSerialization(variable + " as " + n.TypeName, writer, n, builder);
                builder.Write(";");
                builder.NextLine();

                builder.UnIndent();
                builder.RightPar();
            }
            if (children.Length > 0)
            {
                builder.WriteLine("else");
                builder.LeftPar();
                builder.Indent();
                builder.WriteLine("{0}.Write(0);", writer);
                builder.UnIndent();
                builder.RightPar();
            }
        }

        public void VisitSerialization(string variable, string writer, SerializeNode node, TextBuilder builder)
        {
            switch (node.Category)
            {
                case SerializeNode.NodeCategory.PRIMITIVE:
                    VisitPrimitiveSerialization(variable, writer, builder);
                    break;
                case SerializeNode.NodeCategory.ENUM:
                    VisitEnumSerialization(variable, writer, builder);
                    break;
                case SerializeNode.NodeCategory.FIXED:
                    VisitFixedSerialization(variable, writer, builder);
                    break;
                case SerializeNode.NodeCategory.TIMESPAN:
                    VisitTimeSpanSerialization(variable, writer, builder);
                    break;
                case SerializeNode.NodeCategory.DATETIME:
                    VisitDateTimeSerialization(variable, writer, builder);
                    break;
                case SerializeNode.NodeCategory.COLOR:
                    VisitColorSerialization(variable, writer, builder);
                    break;
                case SerializeNode.NodeCategory.NULLABLE:
                    VisitNullableSerialization(variable, writer, node as NullableSerializationNode, builder);
                    break;
                case SerializeNode.NodeCategory.POLY_CLASS:
                case SerializeNode.NodeCategory.CLASS:
                case SerializeNode.NodeCategory.ARRAY:
                case SerializeNode.NodeCategory.LIST:
                case SerializeNode.NodeCategory.DICTIONARY:
                case SerializeNode.NodeCategory.STRUCT:
                    VisitClassSerialization(variable, writer, node, builder);
                    break;
            }
        }

        public void VisitPrimitiveSerialization(string variable, string writer, TextBuilder builder)
        {
            builder.Write("{0}.Write({1})", writer, variable);
        }

        public void VisitEnumSerialization(string variable, string writer, TextBuilder builder)
        {
            builder.Write("{0}.Write((int){1})", writer, variable);
        }

        public virtual void VisitFixedSerialization(string variable, string writer, TextBuilder builder)
        {
            builder.Write("{0}.WriteFixed({1})", writer, variable);
        }

        public void VisitTimeSpanSerialization(string variable, string writer, TextBuilder builder)
        {
            builder.Write("{0}.Write({1}.Ticks / TimeSpan.TicksPerSecond)", writer, variable);
        }

        public void VisitDateTimeSerialization(string variable, string writer, TextBuilder builder)
        {
            builder.Write("{0}.Write((long)TimeUtil.LocalDateToCTime({1}))", writer, variable);
        }

        public void VisitColorSerialization(string variable, string writer, TextBuilder builder)
        {
            builder.Write("{0}.Write((uint){1})", writer, variable);
        }

        public void VisitNullableSerialization(string variable, string writer, NullableSerializationNode node, TextBuilder builder)
        {
            builder.Write("{0}.Write({1} != null);", writer, variable);
            builder.NextLine();
            builder.WriteIndents();
            builder.Write("if({0} != null) ", variable);
            VisitSerialization(variable + ".Value", writer, node.Serialize, builder);
        }

        public void VisitClassSerialization(string variable, string writer, SerializeNode node, TextBuilder builder)
        {
            builder.Write("{0}.Write({1}, {2})", node.TypeNode.SerializerTypeName, writer, variable);
        }

        public void VisitField(string variable, FieldNode node, TextBuilder builder)
        {
            builder.Write(variable);
            builder.Write(".");
            builder.Write(node.FieldName);
        }
    }
}
