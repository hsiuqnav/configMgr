// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class FieldLayoutOfConstSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Kernel.Config.FieldLayoutOfConst)o);
		}
		
		public static Kernel.Config.FieldLayoutOfConst Read(IBinaryReader o, Kernel.Config.FieldLayoutOfConst d)
		{
			if(o.ReadBoolean() == false) return null;
			
			if(d == null) d = new Kernel.Config.FieldLayoutOfConst();
			d.Mode = (Kernel.Config.ConfigFieldInfo.Mode)o.ReadInt32();
			d.Offset = o.ReadInt32();
			d.Size = o.ReadInt32();
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Kernel.Config.FieldLayoutOfConst)o);
		}
		
		public static void Write(IBinaryWriter o, Kernel.Config.FieldLayoutOfConst d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			o.Write((int)d.Mode);
			o.Write(d.Offset);
			o.Write(d.Size);
		}
	}
}
