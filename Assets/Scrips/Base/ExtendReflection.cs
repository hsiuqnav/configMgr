using System;
using System.Reflection;

namespace Kernel
{
	public static class ExtendReflection
	{
		public static bool HasFieldEx<T>(this object obj, string name)
		{
			if (obj == null)
				return false;

			Type t = obj.GetType();
			FieldInfo field = t.GetField(name);
			return field != null && typeof(T).IsAssignableFrom(field.FieldType);
		}

		public static T GetFieldEx<T>(this object obj, string name)
		{
			if (obj == null)
				return default(T);

			Type t = obj.GetType();
			FieldInfo field = t.GetField(name);
			if (field != null && typeof(T).IsAssignableFrom(field.FieldType))
				return (T)field.GetValue(obj);
			else
				return default(T);
		}

		/// <summary>
		/// 获取Field、Property或无参数Method的返回值
		/// </summary>
		public static T GetValueEx<T>(this object obj, string name)
		{
			if (obj == null)
				return default(T);
			Type t = obj.GetType();
			FieldInfo field = t.GetField(name);
			if (field != null && typeof(T).IsAssignableFrom(field.FieldType))
				return (T)field.GetValue(obj);
			PropertyInfo prop = t.GetProperty(name);
			if (prop != null && prop.CanRead)
			{
				MethodInfo m = prop.GetGetMethod();
				if (m != null && typeof(T).IsAssignableFrom(m.ReturnType))
				{
					return (T)m.Invoke(obj, null);
				}
			}
			MethodInfo method = t.GetMethod(name, new Type[0]);
			if (method != null && typeof(T).IsAssignableFrom(method.ReturnType))
			{
				return (T)method.Invoke(obj, null);
			}
			return default(T);
		}

		public static T GetValueEx<T>(this object obj, MemberInfo memberInfo)
		{
			if(memberInfo is FieldInfo)
				return (T)(memberInfo as FieldInfo).GetValue(obj);
			else if(memberInfo is PropertyInfo)
			{
				var prop = memberInfo as PropertyInfo;
				if(prop.CanRead)
				{
					MethodInfo m = prop.GetGetMethod();
					if(m != null && typeof(T).IsAssignableFrom(m.ReturnType))
					{
						return (T)m.Invoke(obj, null);
					}
				}
			}
			return default(T);
		}
	}
}
