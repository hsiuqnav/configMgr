// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class StringSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (string)o);
		}
		
		public static string Read(IBinaryReader o, string d)
		{
			if(o.ReadBoolean() == false) return null;
			
			d = o.ReadString();
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (string)o);
		}
		
		public static void Write(IBinaryWriter o, string d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			o.Write(d);
		}
	}
}
