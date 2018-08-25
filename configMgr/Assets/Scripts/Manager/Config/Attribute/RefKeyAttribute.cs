using Kernel.Config;
using System;
using System.Collections;
using System.Reflection;

namespace Kernel.Lang.Attribute
{
	public class RefKeyAttributeAttribute : ValidateAttribute
	{
		public Type TargetConfType = null; // 目标Config
		public RefKeyAttributeAttribute(Type TargetConfType)
		{
			this.TargetConfType = TargetConfType;
		}

		public override bool Validate(FieldInfo fieldInfo, object fieldValue, Type configType, object configValue, string name, ConfigFieldInfo[] confFieldInfos)
		{
			if (TargetConfType == null)
			{
				return true;
			}
			var configArrayValue = ConfigManager.Instance.GetConfig(TargetConfType) as IDictionary;
			if (configArrayValue == null)
			{
				return true;
			}
			var e = configArrayValue.GetEnumerator();
			while (e.MoveNext())
			{
				var targetConfValue = e.Value;
				var targetConfFieldValue = TypeUtil.FindConfigKeyValue(TargetConfType, targetConfValue);
				if (targetConfFieldValue.Equals(fieldValue))
				{
					return true;
				}
			}
			using(var errorBlock = new ConsoleOutputErrorBlock())
			{
				var targetConfName = string.Empty;
				for(var i = 0; i < confFieldInfos.Length; ++i)
				{
					var f = confFieldInfos[i];
					if (f.ElemType == TargetConfType)
					{
						targetConfName = f.Name + ".xml";
						break;
					}
				}
				Console.WriteLine("{0}.xml element id {1}, refIdField({2}) {3} doesn't found in target {4}",
					name,
					TypeUtil.FindConfigKeyValue(configType, configValue),
					fieldInfo.Name,
					fieldValue,
					string.IsNullOrEmpty(targetConfName) ? TargetConfType.ToString() : targetConfName);
			}
			return false;
		}
	}
}
