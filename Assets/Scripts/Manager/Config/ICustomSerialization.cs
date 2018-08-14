using System;
using System.IO;

namespace Kernel.Config
{
	public interface ICustomSerialization<T>
	{
		void ToSerializedData(IBinaryWriter writer, out T data);
		void FromSerializedData(IBinaryReader reader, T data);
	}

	public class CustomSerialierAttribute : Attribute
	{
		public Type Serializer;

		public CustomSerialierAttribute(Type serializer)
		{
			Serializer = serializer;
		}
	}
}
