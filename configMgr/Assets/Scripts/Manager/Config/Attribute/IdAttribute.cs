using Kernel.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Kernel.Lang.Attribute
{
	public class IdAttribute : ValidateAttribute
	{
		private static HashSet<object> deduplicateBuffer = new HashSet<object>();
		public override bool Validate(FieldInfo fieldInfo, object fieldValue, Type configType, object configValue, string name, ConfigFieldInfo[] confFieldInfos)
		{
			deduplicateBuffer.Clear();
			var configArrayValue = ConfigManager.Instance.GetConfig(configType) as IDictionary;
			var e = configArrayValue.GetEnumerator();
			while (e.MoveNext())
			{
				var v = fieldInfo.GetValue(e.Value);
				if (!deduplicateBuffer.Add(v))
				{
					using(var errorBlock = new ConsoleOutputErrorBlock())
					{
						Console.WriteLine(string.Format("{0}.xml, Id {1} duplicate", name, v));
					}
					return false;
				}
			}
			return true;
		}
	}
}