// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class ArrayColorSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Kernel.Engine.Color[])o);
		}
		
		public static Kernel.Engine.Color[] Read(IBinaryReader o, Kernel.Engine.Color[] d)
		{
			if(o.ReadBoolean() == false) return null;
			
			int size = o.ReadInt32();
			if(d == null || d.Length != size) d = new Kernel.Engine.Color[size];
			for(int i = 0; i < size; ++i)
			{
				d[i] = ColorSerializer.Read(o, d[i]);
			}
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Kernel.Engine.Color[])o);
		}
		
		public static void Write(IBinaryWriter o, Kernel.Engine.Color[] d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			int size = d.Length;
			o.Write(size);
			for(int i = 0; i < size; ++i)
			{
				ColorSerializer.Write(o, d[i]);
			}
		}
	}
}
