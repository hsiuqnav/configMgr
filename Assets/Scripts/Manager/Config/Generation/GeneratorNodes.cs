using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Kernel.Config
{
    public struct RootType
    {
        public readonly Type Type;
        public readonly bool PoliClass;

        public RootType(Type type, bool poliClass)
        {
            Type = type;
            PoliClass = poliClass;
        }

        public RootType(Type type)
        {
            Type = type;
            PoliClass = XmlUtil.IsPolymorphicClass(type);
        }

        public override int GetHashCode()
        {
            return Type != null ? Type.GetHashCode() : 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is RootType)
            {
                var t = (RootType)obj;
                return t.PoliClass == PoliClass && t.Type == Type;
            }
            return false;
        }
    }

    public class RootNode : Node
    {
        public RootType Root;
        public TypeNode Header;
        public ReadPartNode ReadPart;
        public WritePartNode WritePart;

        public string FileName;
        public byte[] CodeMd5;
        public bool IsEntry;
        public SerializeNode[] CustomSerializeParams;

        public bool IsEnum
        {
            get
            {
                return Type.IsEnum;
            }
        }

        public TypeNode ElemType
        {
            get
            {
                if (Header != null && Header.ParameterNodes != null)
                {
                    return Header.ParameterNodes.Last();
                }
                return Header;
            }
        }

        public RootNode(Type type) : base(type)
        {
            Root = new RootType(type);
        }

        public RootNode(RootType type) : base(type.Type)
        {
            Root = type;
        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            if (Type.IsEnum)
            {
                GenerateEnum(referTypes);
                return;
            }

            AddChild(Header = new TypeNode(Root), referTypes);

            List<FieldInfo> fields;
            var customInterface = Type.GetInterfaces().FirstOrDefault(t => t.Name.StartsWith("ICustomSerialization"));
            if (customInterface != null)
            {
                var arguments = customInterface.GetGenericArguments();
                CustomSerializeParams = new SerializeNode[arguments.Length];
                for (int i = 0; i < arguments.Length; i++)
                {
                    AddChild(CustomSerializeParams[i] = SerializeNode.Create(arguments[i]), referTypes);
                }
                fields = new List<FieldInfo>();
            }
            else
            {
                fields = GetSerializeFields(Type);
            }

            AddChild(ReadPart = new ReadPartNode(Root, fields), referTypes);
            AddChild(WritePart = new WritePartNode(Root, fields), referTypes);

            FileName = Header.SerializerTypeName;
        }

        private void GenerateEnum(HashSet<RootType> referTypes)
        {
            AddChild(Header = new TypeNode(Root), referTypes);
            foreach (var name in Enum.GetNames(Type))
            {
                long value = 0;
                var underlyingType = Enum.GetUnderlyingType(Type);
                if (underlyingType == typeof(long))
                {
                    value = (long)Enum.Parse(Type, name);
                }
                else if (underlyingType == typeof(byte))
                {
                    value = (byte)Enum.Parse(Type, name);
                }
                else
                {
                    value = (int)Enum.Parse(Type, name);
                }
                AddChild(new EnumKeyValueNode(Type, name, value), referTypes);
            }
        }
    }

    public class TypeNode : Node
    {
        private static readonly Regex notValidChar = new Regex(@"\W+");

        public string SerializerTypeName
        {
            get
            {
                if (customAttribute != null)
                {
                    return TypeUtil.GetCSharpFullTypeName(customAttribute.Serializer);
                }
                return PureTypeName + "Serializer";
            }
        }

        private string PureTypeName
        {
            get
            {
                if (Type.IsArray)
                {
                    return "Array" + ParameterNodes[0].PureTypeName;
                }
                if (TypeUtil.IsSubclassOfList(Type))
                {
                    return TypeUtil.GetGenericTypeName(Type) + ParameterNodes[0].PureTypeName;
                }
                if (TypeUtil.IsSubclassOfDictionary(Type))
                {
                    return TypeUtil.GetGenericTypeName(Type)
                           + "_" + ParameterNodes[0].PureTypeName
                           + "_" + ParameterNodes[1].PureTypeName;
                }
                if (rootType.PoliClass)
                {
                    return "Poli_" + GetFullNestTypeString(Type);
                }
                return GetFullNestTypeString(Type);
            }
        }

        private bool ShouldExport
        {
            get
            {
                return customAttribute == null && !Type.IsPrimitive
                    && Type != typeof(SerializableTimeSpan)
                    && !TypeUtil.IsNullable(Type);
            }
        }

        public TypeNode[] ParameterNodes;

        protected RootType rootType;

        private CustomSerialierAttribute customAttribute;

        public TypeNode(Type type, CustomSerialierAttribute customAttr = null) : base(type)
        {
            rootType = new RootType(type, XmlUtil.IsPolymorphicClass(type));
            customAttribute = customAttr ?? TypeUtil.GetCustomAttribute<CustomSerialierAttribute>(type, true);
        }

        public TypeNode(RootType type) : base(type.Type)
        {
            rootType = type;
            customAttribute = TypeUtil.GetCustomAttribute<CustomSerialierAttribute>(type.Type, true);
        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            if (!ShouldExport) return;
            if (!referTypes.Contains(rootType)) referTypes.Add(rootType);

            if (Type.IsArray)
            {
                ParameterNodes = new TypeNode[1];
                AddChild(ParameterNodes[0] = new TypeNode(TypeUtil.GetArrayValueType(Type)), referTypes);
            }
            else if (TypeUtil.IsSubclassOfList(Type))
            {
                ParameterNodes = new TypeNode[1];
                AddChild(ParameterNodes[0] = new TypeNode(TypeUtil.GetListValueType(Type)), referTypes);
            }
            else if (TypeUtil.IsSubclassOfDictionary(Type))
            {
                ParameterNodes = new TypeNode[2];
                AddChild(ParameterNodes[0] = new TypeNode(TypeUtil.GetDictionaryKeyType(Type)), referTypes);
                AddChild(ParameterNodes[1] = new TypeNode(TypeUtil.GetDictionaryValueType(Type)), referTypes);
            }
            else if (TypeUtil.IsSubclassOfHashSet(Type))
            {
                ParameterNodes = new TypeNode[1];
                AddChild(ParameterNodes[0] = new TypeNode(TypeUtil.GetHashSetValueType(Type)), referTypes);
            }
            else if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(SerializableTwoDimArray<>))
            {
                ParameterNodes = new TypeNode[1];
                AddChild(ParameterNodes[0] = new TypeNode(TypeUtil.GetArrayValueType(TypeUtil.GetArrayValueType(Type))), referTypes);
            }
        }

        protected string GetFullNestTypeString(Type type)
        {
            return notValidChar.Replace(TypeUtil.GetTypeNameWithNest(type), "_");
        }
    }

    public class FieldNode : Node
    {
        public TypeNode FieldType;
        public SerializeNode Serialize;

        private FieldInfo field;
        private CustomSerialierAttribute customAttribute;

        public string FieldName
        {
            get
            {
                return field.Name;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return field.IsInitOnly;
            }
        }

        public FieldNode(Type type, FieldInfo field) : base(type)
        {
            this.field = field;
            customAttribute = TypeUtil.GetCustomAttribute<CustomSerialierAttribute>(field, true);
        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            AddChild(FieldType = new TypeNode(Type, customAttribute), referTypes);
            AddChild(Serialize = SerializeNode.Create(field.FieldType, customAttribute), referTypes);
        }
    }

    public class ArrayTypeNode : TypeNode
    {
        public SerializeNode Serialize;

        public ArrayTypeNode(Type type) : base(type)
        {

        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            base.Generate(referTypes);
            AddChild(Serialize = SerializeNode.Create(TypeUtil.GetArrayValueType(Type)), referTypes);
        }
    }

    public class TwoDimArrayTypeNode : TypeNode
    {
        public SerializeNode Serialize;

        public TwoDimArrayTypeNode(Type type) : base(type)
        {

        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            base.Generate(referTypes);
            AddChild(Serialize = SerializeNode.Create(TypeUtil.GetArrayValueType(TypeUtil.GetArrayValueType(Type))), referTypes);
        }
    }

    public class ListTypeNode : TypeNode
    {
        public SerializeNode Serialize;

        public ListTypeNode(Type type) : base(type)
        {

        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            base.Generate(referTypes);
            AddChild(Serialize = SerializeNode.Create(TypeUtil.GetListValueType(Type)), referTypes);
        }
    }

    public class HashSetTypeNode : TypeNode
    {
        public SerializeNode Serialize;

        public HashSetTypeNode(Type type) : base(type)
        {

        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            base.Generate(referTypes);
            AddChild(Serialize = SerializeNode.Create(TypeUtil.GetHashSetValueType(Type)), referTypes);
        }
    }

    public class DictionaryTypeNode : TypeNode
    {
        public SerializeNode SerializeKey;
        public SerializeNode SerializeValue;

        public DictionaryTypeNode(Type type) : base(type)
        {

        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            base.Generate(referTypes);
            AddChild(SerializeKey = SerializeNode.Create(TypeUtil.GetDictionaryKeyType(Type)), referTypes);
            AddChild(SerializeValue = SerializeNode.Create(TypeUtil.GetDictionaryValueType(Type)), referTypes);
        }
    }

    public class PolyClassTypeNode : TypeNode
    {
        public PolyClassTypeNode(Type type) : base(type)
        {

        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            base.Generate(referTypes);

            foreach (var subType in GetSortedNoneAbstractTypes(Type))
            {
                if (subType == Type)
                {
                    var typeNode = new TypeNode(new RootType(subType, false));
                    var serializeNode = new SerializeNode(subType, SerializeNode.NodeCategory.CLASS, typeNode);

                    AddChild(serializeNode, referTypes);
                }
                else
                {
                    AddChild(SerializeNode.Create(subType), referTypes);
                }
            }
        }

        private IEnumerable<Type> GetSortedNoneAbstractTypes(Type type)
        {
            var list = TypeUtil
                .GetXmlIncludeTypes(type)
                .Union(new[]
                {
                    type
                })
                .Where(o => o != type && !o.IsAbstract)
                .OrderBy(o => o.Name).ToList();

            if (!type.IsAbstract)
            {
                list.Insert(0, type);
            }

            return list;
        }
    }

    public class StringTypeNode : TypeNode
    {
        public StringTypeNode(Type type) : base(type)
        {

        }
    }

    public class EnumKeyValueNode : Node
    {
        public readonly string Key;
        public readonly long Value;

        public EnumKeyValueNode(Type type, string key, long value) : base(type)
        {
            Key = key;
            Value = value;
        }
    }

    public class ReadPartNode : Node
    {
        public readonly bool CreateNew;

        private List<FieldInfo> fields;
        private RootType rootType;

        public ReadPartNode(RootType type, List<FieldInfo> fields) : base(type.Type)
        {
            this.fields = fields;
            rootType = type;
            CreateNew = type.Type.IsClass && type.Type.GetConstructor(new Type[0]) != null;
        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            if (Type.IsArray)
            {
                AddChild(new ArrayTypeNode(Type), referTypes);
            }
            else if (TypeUtil.IsSubclassOfList(Type))
            {
                AddChild(new ListTypeNode(Type), referTypes);
            }
            else if (TypeUtil.IsSubclassOfDictionary(Type))
            {
                AddChild(new DictionaryTypeNode(Type), referTypes);
            }
            else if (TypeUtil.IsSubclassOfHashSet(Type))
            {
                AddChild(new HashSetTypeNode(Type), referTypes);
            }
            else if (rootType.PoliClass)
            {
                AddChild(new PolyClassTypeNode(Type), referTypes);
            }
            else if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(SerializableTwoDimArray<>))
            {
                AddChild(new TwoDimArrayTypeNode(Type), referTypes);
            }
            else if (Type == typeof(string))
            {
                AddChild(new StringTypeNode(Type), referTypes);
            }
            else
            {
                foreach (var field in fields)
                {
                    AddChild(new FieldNode(field.FieldType, field), referTypes);
                }
            }
        }
    }

    public class WritePartNode : Node
    {
        private List<FieldInfo> fields;
        private RootType rootType;

        public WritePartNode(RootType type, List<FieldInfo> fields) : base(type.Type)
        {
            this.fields = fields;
            rootType = type;
        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            if (Type.IsArray)
            {
                AddChild(new ArrayTypeNode(Type), referTypes);
            }
            else if (TypeUtil.IsSubclassOfList(Type))
            {
                AddChild(new ListTypeNode(Type), referTypes);
            }
            else if (TypeUtil.IsSubclassOfDictionary(Type))
            {
                AddChild(new DictionaryTypeNode(Type), referTypes);
            }
            else if (TypeUtil.IsSubclassOfHashSet(Type))
            {
                AddChild(new HashSetTypeNode(Type), referTypes);
            }
            else if (rootType.PoliClass)
            {
                AddChild(new PolyClassTypeNode(Type), referTypes);
            }
            else if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(SerializableTwoDimArray<>))
            {
                AddChild(new TwoDimArrayTypeNode(Type), referTypes);
            }
            else if (Type == typeof(string))
            {
                AddChild(new StringTypeNode(Type), referTypes);
            }
            else
            {
                foreach (var field in fields)
                {
                    AddChild(new FieldNode(field.FieldType, field), referTypes);
                }
            }
        }
    }

    public class SerializeNode : Node
    {
        public enum NodeCategory
        {
            PRIMITIVE,
            STRING,
            LOCALE,
            ENUM,
            CLASS,
            POLY_CLASS,
            STRUCT,
            ARRAY,
            LIST,
            DICTIONARY,
            TIMESPAN,
            DATETIME,
            COLOR,
            NULLABLE,
        }

        public TypeNode TypeNode;

        public NodeCategory Category;

        private CustomSerialierAttribute customAttribute;

        public SerializeNode(Type type, NodeCategory nodeCategory, TypeNode typeNode = null,
            CustomSerialierAttribute customAttr = null) : base(type)
        {
            Category = nodeCategory;
            TypeNode = typeNode;
            customAttribute = customAttr;
        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            AddChild(TypeNode ?? (TypeNode = new TypeNode(Type, customAttribute)), referTypes);
        }

        public static SerializeNode Create(Type fieldType, CustomSerialierAttribute customAttribute = null)
        {
            var type = fieldType;
            if (type.IsArray)
            {
                return new SerializeNode(type, NodeCategory.ARRAY, customAttr: customAttribute);
            }
            if (type == typeof(SerializableTimeSpan))
            {
                return new SerializeNode(type, NodeCategory.TIMESPAN, customAttr: customAttribute);
            }
            if (type == typeof(DateTime))
            {
                return new SerializeNode(type, NodeCategory.DATETIME, customAttr: customAttribute);
            }
            if (TypeUtil.IsSubclassOfDictionary(type))
            {
                return new SerializeNode(type, NodeCategory.DICTIONARY, customAttr: customAttribute);
            }
            if (TypeUtil.IsSubclassOfList(type))
            {
                return new SerializeNode(type, NodeCategory.LIST, customAttr: customAttribute);
            }
            if (TypeUtil.IsNullable(type))
            {
                return new NullableSerializationNode(type, Create(TypeUtil.GetNullableValueType(type)), customAttribute);
            }
            if (type.IsPrimitive)
            {
                return new SerializeNode(type, NodeCategory.PRIMITIVE, customAttr: customAttribute);
            }
            if (type.IsEnum)
            {
                return new SerializeNode(type, NodeCategory.ENUM, customAttr: customAttribute);
            }
            if (XmlUtil.IsPolymorphicClass(type))
            {
                return new SerializeNode(type, NodeCategory.POLY_CLASS, customAttr: customAttribute);
            }
            if (type.IsClass)
            {
                return new SerializeNode(type, NodeCategory.CLASS, customAttr: customAttribute);
            }
            if (type.IsValueType)
            {
                return new SerializeNode(type, NodeCategory.STRUCT, customAttr: customAttribute);
            }
            return null;
        }
    }

    public class NullableSerializationNode : SerializeNode
    {
        public SerializeNode Serialize;

        public NullableSerializationNode(Type type, SerializeNode subTypeNode, CustomSerialierAttribute customAttribute)
            : base(type, NodeCategory.NULLABLE, customAttr: customAttribute)
        {
            Serialize = subTypeNode;
        }

        public override void Generate(HashSet<RootType> referTypes)
        {
            Serialize.Generate(referTypes);
        }
    }
}
