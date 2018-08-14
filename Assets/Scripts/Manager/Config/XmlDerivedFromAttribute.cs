using System;

namespace Kernel.Config
{
	[AttributeUsage(AttributeTargets.Class)]
	public class XmlDerivedFromAttribute : Attribute
	{
		public readonly Type BaseType;

		public XmlDerivedFromAttribute(Type type)
		{
			BaseType = type;
		}
	}
}
