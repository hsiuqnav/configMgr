// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class Poli_FieldLayoutSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Kernel.Config.FieldLayout)o);
		}
		
		public static Kernel.Config.FieldLayout Read(IBinaryReader o, Kernel.Config.FieldLayout d)
		{
			if(o.ReadBoolean() == false) return null;
			
			int type = o.ReadInt32();
			switch(type)
			{
				case 1:
					d = FieldLayoutOfConstSerializer.Read(o, d as Kernel.Config.FieldLayoutOfConst);
					break;
				case 2:
					d = FieldLayoutOfIntKeySerializer.Read(o, d as Kernel.Config.FieldLayoutOfIntKey);
					break;
				case 3:
					d = FieldLayoutOfStringKeySerializer.Read(o, d as Kernel.Config.FieldLayoutOfStringKey);
					break;
			}
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Kernel.Config.FieldLayout)o);
		}
		
		public static void Write(IBinaryWriter o, Kernel.Config.FieldLayout d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			Type type = d.GetType();
			if(type == typeof(Kernel.Config.FieldLayoutOfConst))
			{
				o.Write(1);
				FieldLayoutOfConstSerializer.Write(o, d as Kernel.Config.FieldLayoutOfConst);
			}
			else if(type == typeof(Kernel.Config.FieldLayoutOfIntKey))
			{
				o.Write(2);
				FieldLayoutOfIntKeySerializer.Write(o, d as Kernel.Config.FieldLayoutOfIntKey);
			}
			else if(type == typeof(Kernel.Config.FieldLayoutOfStringKey))
			{
				o.Write(3);
				FieldLayoutOfStringKeySerializer.Write(o, d as Kernel.Config.FieldLayoutOfStringKey);
			}
			else
			{
				o.Write(0);
			}
		}
	}
}
