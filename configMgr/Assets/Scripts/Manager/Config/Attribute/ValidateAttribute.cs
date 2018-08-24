using Kernel.Config;
using System;
using System.Reflection;

namespace Kernel.Lang.Attribute
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public abstract class ValidateAttribute : System.Attribute
	{
		public virtual bool Validate(FieldInfo fieldInfo, object fieldValue, Type configType, object configValue, string name, ConfigFieldInfo[] confFieldInfos)
		{
			return true;
		}
	}

	public class ConsoleOutputErrorBlock : IDisposable
	{
		public ConsoleOutputErrorBlock()
		{
#if CODE_GEN
			Console.ForegroundColor = ConsoleColor.Red;
#endif
		}
		public void Dispose()
		{
#if CODE_GEN
			Console.ResetColor();
#endif
		}
	}
}