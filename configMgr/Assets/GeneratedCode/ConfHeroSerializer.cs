// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class ConfHeroSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Config.ConfHero)o);
		}
		
		public static Config.ConfHero Read(IBinaryReader o, Config.ConfHero d)
		{
			if(o.ReadBoolean() == false) return null;
			
			if(d == null) d = new Config.ConfHero();
			d.Attack = o.ReadSingle();
			d.BirthDay = TimeUtil.CTimeToUtcDate(o.ReadInt64());
			d.HeroDesc = StringSerializer.Read(o, d.HeroDesc as string) as string;
			d.HeroName = StringSerializer.Read(o, d.HeroName as string) as string;
			d.Hp = o.ReadSingle();
			d.Id = o.ReadInt32();
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Config.ConfHero)o);
		}
		
		public static void Write(IBinaryWriter o, Config.ConfHero d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			o.Write(d.Attack);
			o.Write((long)TimeUtil.LocalDateToCTime(d.BirthDay));
			StringSerializer.Write(o, d.HeroDesc);
			StringSerializer.Write(o, d.HeroName);
			o.Write(d.Hp);
			o.Write(d.Id);
		}
	}
}
