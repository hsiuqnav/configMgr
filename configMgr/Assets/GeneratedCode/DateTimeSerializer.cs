// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class DateTimeSerializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (System.DateTime)o);
		}
		
		public static System.DateTime Read(IBinaryReader o, System.DateTime d)
		{
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (System.DateTime)o);
		}
		
		public static void Write(IBinaryWriter o, System.DateTime d)
		{
		}
	}
}
