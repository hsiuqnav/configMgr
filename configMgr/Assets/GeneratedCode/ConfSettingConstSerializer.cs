// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class ConfSettingConstSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Config.ConfSettingConst)o);
		}
		
		public static Config.ConfSettingConst Read(IBinaryReader o, Config.ConfSettingConst d)
		{
			if(o.ReadBoolean() == false) return null;
			
			if(d == null) d = new Config.ConfSettingConst();
			d.Locale = StringSerializer.Read(o, d.Locale as string) as string;
			d.UseAA = o.ReadBoolean();
			d.Version = StringSerializer.Read(o, d.Version as string) as string;
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Config.ConfSettingConst)o);
		}
		
		public static void Write(IBinaryWriter o, Config.ConfSettingConst d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			StringSerializer.Write(o, d.Locale);
			o.Write(d.UseAA);
			StringSerializer.Write(o, d.Version);
		}
	}
}
