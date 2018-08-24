using Kernel.Config;
using System;
using System.Collections;
using System.Reflection;

namespace Kernel.Lang.Attribute
{
	public class RefIdAttribute : ValidateAttribute
	{
		public Type TargetConfType = null; // 目标Config
		public RefIdAttribute(Type TargetConfType)
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
				var targetConfFieldValue = GetTargetConfId(TargetConfType, targetConfValue);
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
				Console.WriteLine("{0}.xml refId field {1} {2} doesn't found in target {3}",
					name,
					fieldInfo.Name,
					fieldValue,
					string.IsNullOrEmpty(targetConfName) ? TargetConfType.ToString() : targetConfName);
			}
			return false;
		}

		private object GetTargetConfId(Type t, object v)
		{
			var serializeFields = TypeUtil.GetSerializedFields(t);
			for(var i = 0; i < serializeFields.Count; ++i)
			{
				var field = serializeFields[i];
				var attri = TypeUtil.GetCustomAttribute<IdAttribute>(field, false);
				if (attri != null)
				{
					return field.GetValue(v);
				}
			}
			return null;
		}
	}
}
