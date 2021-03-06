// Auto generated code
using Kernel;
using Kernel.Config;
using Kernel.Util;
using System;
using System.IO;

namespace GeneratedCode
{
	public class Dictionary_Int32_Int32Serializer : IConfigSerializer
	{
		object IConfigSerializer.Read(IBinaryReader reader, object o)
		{
			return Read(reader, (System.Collections.Generic.Dictionary<int,int>)o);
		}
		
		public static System.Collections.Generic.Dictionary<int,int> Read(IBinaryReader o, System.Collections.Generic.Dictionary<int,int> d)
		{
			if(o.ReadBoolean() == false) return null;
			
			int size = o.ReadInt32();
			if(d == null) 
				d = new System.Collections.Generic.Dictionary<int,int>(size, Int32EqualityComparer.Instance);
			else
				d.Clear();
			for(int i = 0; i < size; ++i)
			{
				int key = o.ReadInt32();
				int value = o.ReadInt32();
				d.Add(key, value);
			}
			return d;
		}
		
		void IConfigSerializer.Write(IBinaryWriter writer, object o)
		{
			Write(writer, (System.Collections.Generic.Dictionary<int,int>)o);
		}
		
		public static void Write(IBinaryWriter o, System.Collections.Generic.Dictionary<int,int> d)
		{
			o.Write(d != null);
			if(d == null) return;
			
			int size = d.Count;
			o.Write(size);
			foreach(var elem in d)
			{
				o.Write(elem.Key);
				o.Write(elem.Value);
			}
		}
	}
}
