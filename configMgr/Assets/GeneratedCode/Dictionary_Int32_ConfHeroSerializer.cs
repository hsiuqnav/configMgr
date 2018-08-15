// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class Dictionary_Int32_ConfHeroSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (System.Collections.Generic.Dictionary<int,Config.ConfHero>)o);
		}
		
		public static System.Collections.Generic.Dictionary<int,Config.ConfHero> Read(IBinaryReader o, System.Collections.Generic.Dictionary<int,Config.ConfHero> d)
		{
			if(o.ReadBoolean() == false) return null;
			
			int size = o.ReadInt32();
			if(d == null) 
				d = new System.Collections.Generic.Dictionary<int,Config.ConfHero>(size, Int32EqualityComparer.Instance);
			else
				d.Clear();
			for(int i = 0; i < size; ++i)
			{
				int key = o.ReadInt32();
				Config.ConfHero value = ConfHeroSerializer.Read(o, default(Config.ConfHero) as Config.ConfHero);
				d.Add(key, value);
			}
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (System.Collections.Generic.Dictionary<int,Config.ConfHero>)o);
		}
		
		public static void Write(IBinaryWriter o, System.Collections.Generic.Dictionary<int,Config.ConfHero> d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			int size = d.Count;
			o.Write(size);
			foreach(var elem in d)
			{
				o.Write(elem.Key);
				ConfHeroSerializer.Write(o, elem.Value);
			}
		}
	}
}
