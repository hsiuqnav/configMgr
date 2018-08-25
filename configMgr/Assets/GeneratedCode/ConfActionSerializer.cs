// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class ConfActionSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Config.ConfAction)o);
		}
		
		public static Config.ConfAction Read(IBinaryReader o, Config.ConfAction d)
		{
			if(o.ReadBoolean() == false) return null;
			
			if(d == null) d = new Config.ConfAction();
			d.ActionLength = o.ReadSingle();
			d.ActionName = StringSerializer.Read(o, d.ActionName as string) as string;
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Config.ConfAction)o);
		}
		
		public static void Write(IBinaryWriter o, Config.ConfAction d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			o.Write(d.ActionLength);
			StringSerializer.Write(o, d.ActionName);
		}
	}
}
