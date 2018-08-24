using System;
using System.Reflection;

namespace Kernel.Lang.Attribute
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public abstract class ValidateAttribute : System.Attribute
	{
		public virtual bool Validate(FieldInfo fieldInfo, object fieldValue, Type configType, object configValue, string name)
		{
			return true;
		}
	}
}