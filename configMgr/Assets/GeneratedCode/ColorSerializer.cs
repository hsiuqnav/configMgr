// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class ColorSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (Kernel.Engine.Color)o);
		}
		
		public static Kernel.Engine.Color Read(IBinaryReader o, Kernel.Engine.Color d)
		{
			d.a = o.ReadSingle();
			d.b = o.ReadSingle();
			d.g = o.ReadSingle();
			d.r = o.ReadSingle();
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (Kernel.Engine.Color)o);
		}
		
		public static void Write(IBinaryWriter o, Kernel.Engine.Color d)
		{
			o.Write(d.a);
			o.Write(d.b);
			o.Write(d.g);
			o.Write(d.r);
		}
	}
}
