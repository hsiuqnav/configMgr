using System;
using System.Reflection;

namespace Kernel.Config
{
	[Flags]
	public enum ConfigExportPolicy
	{
		EXPORT_TO_CS = 1,
		EXPORT_TO_LUA = 2,
		EXPORT_TO_BOTH = EXPORT_TO_LUA | EXPORT_TO_CS,
	}

	public abstract class ConfigAttribute : Attribute
	{
		public string Comment;
		public string Name;
		public ConfigExportPolicy ExportPolicy = ConfigExportPolicy.EXPORT_TO_CS;
		public bool Boot;
		public bool ExportXmlExample = true;

		public virtual Type GetConfigType(Type classType)
		{
			return classType;
		}

		public bool ExportToLua
		{
			get
			{
				return ExportPolicy == ConfigExportPolicy.EXPORT_TO_LUA || ExportPolicy == ConfigExportPolicy.EXPORT_TO_BOTH;
			}
		}

		public bool ExportToCS
		{
			get
			{
				return ExportPolicy == ConfigExportPolicy.EXPORT_TO_CS || ExportPolicy == ConfigExportPolicy.EXPORT_TO_BOTH;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class ConstConfigAttribute : ConfigAttribute
	{

	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class DictionaryConfigAttribute : ConfigAttribute
	{
		public string Key = "Id";
		public bool LoadAll;

		public override Type GetConfigType(Type classType)
		{
			var field = classType.GetField(Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
			if (field != null)
			{
				return TypeUtil.MakeDictionaryType(field.FieldType, classType);
			}

			var property = classType.GetProperty(Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
			if (property != null)
			{
				return TypeUtil.MakeDictionaryType(property.PropertyType, classType);
			}

			throw new Exception("Config key not found " + classType + " " + Key);
		}
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class ArrayConfigAttribute : ConfigAttribute
	{
		public override Type GetConfigType(Type classType)
		{
			return classType.MakeArrayType();
		}
	}

	[AttributeUsage(AttributeTargets.Enum, Inherited = false)]
	public class EnumConfigAttribute : ConfigAttribute
	{
		public Type[] CompatiableTypes;

		public EnumConfigAttribute()
		{

		}

		public EnumConfigAttribute(params Type[] compatiableTypes)
		{
			CompatiableTypes = compatiableTypes;
		}
	}
}
