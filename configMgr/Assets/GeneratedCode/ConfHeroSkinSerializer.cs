// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class ConfHeroSkinSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Config.ConfHeroSkin)o);
		}
		
		public static Config.ConfHeroSkin Read(IBinaryReader o, Config.ConfHeroSkin d)
		{
			if(o.ReadBoolean() == false) return null;
			
			if(d == null) d = new Config.ConfHeroSkin();
			d.Desc = StringSerializer.Read(o, d.Desc as string) as string;
			d.HeroId = o.ReadInt32();
			d.Id = o.ReadInt32();
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Config.ConfHeroSkin)o);
		}
		
		public static void Write(IBinaryWriter o, Config.ConfHeroSkin d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			StringSerializer.Write(o, d.Desc);
			o.Write(d.HeroId);
			o.Write(d.Id);
		}
	}
}
