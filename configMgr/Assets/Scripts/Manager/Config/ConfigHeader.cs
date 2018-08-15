using System;
using System.Collections;
using System.Collections.Generic;

namespace Kernel.Config
{
    public class ConfigHeader
    {
        public int HeaderSize;
        public int BodySize;
        public FieldLayout[] Contents;
    }

#if CODE_GEN
	// 不用XmlDerived的原因是因为有IDictionary的属性，XmlSerializer会报错
	[System.Xml.Serialization.XmlInclude(typeof(FieldLayoutOfConst))]
	[System.Xml.Serialization.XmlInclude(typeof(FieldLayoutOfIntKey))]
	[System.Xml.Serialization.XmlInclude(typeof(FieldLayoutOfStringKey))]
#endif

    public abstract class FieldLayout
    {
        public ConfigFieldInfo.Mode Mode;
        public int Offset;
        public int Size;

        public static FieldLayout CreateFieldLayout(ConfigFieldInfo.Mode mode, int offset, Type keyType)
        {
            if (mode == ConfigFieldInfo.Mode.KEY_VALUE)
            {
                if (keyType == typeof(int) || keyType.IsEnum)
                    return new FieldLayoutOfIntKey { Mode = mode, Offset = offset };
                if (keyType == typeof(string))
                    return new FieldLayoutOfStringKey { Mode = mode, Offset = offset };
            }
            return new FieldLayoutOfConst { Mode = ConfigFieldInfo.Mode.CONST, Offset = offset };
        }

        public abstract IDictionary OffsetDict
        {
            get;
        }

        public abstract void Add(object key, int off);
    }

    public class FieldLayoutOfConst : FieldLayout
    {
        public override IDictionary OffsetDict
        {
            get
            {
                return null;
            }
        }

        public override void Add(object key, int off)
        {

        }
    }

    public class FieldLayoutOfIntKey : FieldLayout
    {
        public Dictionary<int, int> ElemOffset = new Dictionary<int, int>(Int32EqualityComparer.Instance);

        public override IDictionary OffsetDict
        {
            get
            {
                return ElemOffset;
            }
        }

        public override void Add(object key, int off)
        {
            ElemOffset.Add((int)key, off);
        }
    }

    public class FieldLayoutOfStringKey : FieldLayout
    {
        public Dictionary<string, int> ElemOffset = new Dictionary<string, int>(StringComparer.Ordinal);

        public override IDictionary OffsetDict
        {
            get
            {
                return ElemOffset;
            }
        }

        public override void Add(object key, int off)
        {
            ElemOffset.Add((string)key, off);
        }
    }
}
