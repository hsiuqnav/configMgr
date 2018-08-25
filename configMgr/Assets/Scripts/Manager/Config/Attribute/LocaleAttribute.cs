using Kernel.Config;
using System;
using System.Reflection;

namespace Kernel.Lang.Attribute
{
	public class LocaleAttribute : ValidateAttribute
	{
		public override bool Validate(FieldInfo fieldInfo, object fieldValue, Type configType, object configValue, string name, ConfigFieldInfo[] confFieldInfos)
		{
			var key = fieldValue as string;
			if (string.IsNullOrEmpty(key))
			{
				using(var block = new ConsoleOutputErrorBlock())
				{
					Console.WriteLine("{0}.xml elementId {1} 发现不存在的locale", name, TypeUtil.FindConfigKeyValue(configType, configValue));
				}
			}
			if (!Locale.ContainsKey(key))
			{
				using (var block = new ConsoleOutputErrorBlock())
				{
					Console.WriteLine("{0}.xml elementId {1} 发现不存在的locale {2}", name, TypeUtil.FindConfigKeyValue(configType, configValue), fieldValue);
				}
			}
			return true;
		}
	}
}
