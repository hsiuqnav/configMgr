// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class FieldLayoutOfStringKeySerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Kernel.Config.FieldLayoutOfStringKey)o);
		}
		
		public static Kernel.Config.FieldLayoutOfStringKey Read(IBinaryReader o, Kernel.Config.FieldLayoutOfStringKey d)
		{
			if(o.ReadBoolean() == false) return null;
			
			if(d == null) d = new Kernel.Config.FieldLayoutOfStringKey();
			d.ElemOffset = Dictionary_String_Int32Serializer.Read(o, d.ElemOffset as System.Collections.Generic.Dictionary<string,int>);
			d.Mode = (Kernel.Config.ConfigFieldInfo.Mode)o.ReadInt32();
			d.Offset = o.ReadInt32();
			d.Size = o.ReadInt32();
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Kernel.Config.FieldLayoutOfStringKey)o);
		}
		
		public static void Write(IBinaryWriter o, Kernel.Config.FieldLayoutOfStringKey d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			Dictionary_String_Int32Serializer.Write(o, d.ElemOffset);
			o.Write((int)d.Mode);
			o.Write(d.Offset);
			o.Write(d.Size);
		}
	}
}
