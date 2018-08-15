// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class ArrayPoli_FieldLayoutSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Kernel.Config.FieldLayout[])o);
		}
		
		public static Kernel.Config.FieldLayout[] Read(IBinaryReader o, Kernel.Config.FieldLayout[] d)
		{
			if(o.ReadBoolean() == false) return null;
			
			int size = o.ReadInt32();
			if(d == null || d.Length != size) d = new Kernel.Config.FieldLayout[size];
			for(int i = 0; i < size; ++i)
			{
				d[i] = Poli_FieldLayoutSerializer.Read(o, d[i] as Kernel.Config.FieldLayout);
			}
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Kernel.Config.FieldLayout[])o);
		}
		
		public static void Write(IBinaryWriter o, Kernel.Config.FieldLayout[] d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			int size = d.Length;
			o.Write(size);
			for(int i = 0; i < size; ++i)
			{
				Poli_FieldLayoutSerializer.Write(o, d[i]);
			}
		}
	}
}
