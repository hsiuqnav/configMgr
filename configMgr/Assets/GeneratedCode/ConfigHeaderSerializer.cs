// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class ConfigHeaderSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Kernel.Config.ConfigHeader)o);
		}
		
		public static Kernel.Config.ConfigHeader Read(IBinaryReader o, Kernel.Config.ConfigHeader d)
		{
			if(o.ReadBoolean() == false) return null;
			
			if(d == null) d = new Kernel.Config.ConfigHeader();
			d.BodySize = o.ReadInt32();
			d.Contents = ArrayPoli_FieldLayoutSerializer.Read(o, d.Contents as Kernel.Config.FieldLayout[]);
			d.HeaderSize = o.ReadInt32();
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Kernel.Config.ConfigHeader)o);
		}
		
		public static void Write(IBinaryWriter o, Kernel.Config.ConfigHeader d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			o.Write(d.BodySize);
			ArrayPoli_FieldLayoutSerializer.Write(o, d.Contents);
			o.Write(d.HeaderSize);
		}
	}
}
