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
			d.Desc = StringSerializer.Read(o, d.Desc as string) as string;
			d.Hp = o.ReadSingle();
			d.Id = o.ReadInt32();
			d.Name = StringSerializer.Read(o, d.Name as string) as string;
			d.PrefabPath = StringSerializer.Read(o, d.PrefabPath as string) as string;
			d.Quality = (Config.QualificationType)o.ReadInt32();
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
			StringSerializer.Write(o, d.Desc);
			o.Write(d.Hp);
			o.Write(d.Id);
			StringSerializer.Write(o, d.Name);
			StringSerializer.Write(o, d.PrefabPath);
			o.Write((int)d.Quality);
		}
	}
}
