using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Kernel.Util;
using Kernel.Lang.Attribute;
using Kernel.Config;

namespace Kernel
{
	public struct FieldAndAttribute<T> where T : Attribute
	{
		public Type fieldType;
		public string fieldName;
		public T attribute;
	}

	public static class TypeUtil
	{
		private static readonly Type[] EmptyTypeList = new Type[0];
		private static readonly Type[] ParamInt = { typeof(int) };
		private static readonly Type[] ParamDict = { typeof(IDictionary<int, object>) };
		private static readonly Type DictionaryGenericType = typeof(Dictionary<,>);
		private static readonly Type ListGenericType = typeof(List<>);
		private static readonly Type HashSetGenericType = typeof(HashSet<>);
		private static readonly Type DelegationType = typeof(Delegate);
		private static readonly Type NullableType = typeof(Nullable<>);

		////////////////////////////////////
		// list
		public static bool IsList(Type type)
		{
			return IsGenericType(type) && type.GetGenericTypeDefinition() == ListGenericType;
		}

		public static bool IsSubclassOfList(Type type)
		{
			return FindGenerticAncestor(type, ListGenericType) != null;
		}

		public static bool IsNullable(Type type)
		{
			return FindGenerticAncestor(type, NullableType) != null;
		}

		public static Type GetListValueType(Type type)
		{
			return FindGenerticAncestor(type, ListGenericType).GetGenericArguments()[0];
		}

		public static Type GetArrayValueType(Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SerializableTwoDimArray<>))
			{
				Type t = type.GetGenericArguments().FirstOrDefault();
				if (t != null)
				{
					return t.MakeArrayType();
				}
			}
			return type.GetElementType();
		}

		public static Type GetNullableValueType(Type type)
		{
			return FindGenerticAncestor(type, NullableType).GetGenericArguments()[0];
		}

		////////////////////////////////////
		// hashset
		public static bool IsSubclassOfHashSet(Type type)
		{
			return FindGenerticAncestor(type, HashSetGenericType) != null;
		}

		public static Type GetHashSetValueType(Type type)
		{
			return FindGenerticAncestor(type, HashSetGenericType).GetGenericArguments()[0];
		}

		////////////////////////////////////
		// dictionary
		public static bool IsDictionary(Type type)
		{
			return IsGenericType(type) && type.GetGenericTypeDefinition() == DictionaryGenericType;
		}

		public static bool IsSubclassOfDictionary(Type type)
		{
			return FindGenerticAncestor(type, DictionaryGenericType) != null;
		}

		public static Type GetDictionaryKeyType(Type type)
		{
			return FindGenerticAncestor(type, DictionaryGenericType).GetGenericArguments()[0];
		}

		public static Type GetDictionaryValueType(Type type)
		{
			return FindGenerticAncestor(type, DictionaryGenericType).GetGenericArguments()[1];
		}

		////////////////////////////////////
		// others
		public static bool IsCollection(Type type)
		{
			return IsSubclassOfList(type) || IsSubclassOfDictionary(type);
		}

		public static bool IsDelegation(Type type)
		{
			return type.IsSubclassOf(DelegationType);
		}

		private static Dictionary<Type, string> serializeTypeNames = new Dictionary<Type, string>
		{
			{ typeof(int), "int" },
			{ typeof(uint), "uint" },
			{ typeof(long), "long" },
			{ typeof(ulong), "ulong" },
			{ typeof(short), "short" },
			{ typeof(ushort), "ushort" },
			{ typeof(byte), "byte" },
			{ typeof(sbyte), "sbyte" },
			{ typeof(string), "string" },
			{ typeof(float), "float" },
			{ typeof(double), "double" },
			{ typeof(bool), "bool" },
		};

		private static readonly Regex notValidChar = new Regex(@"\W+");

		public static string ReplacePrimitiveTypeName(string typeName)
		{
			foreach (var pair in serializeTypeNames)
			{
				typeName = typeName.Replace(pair.Key.FullName, pair.Value);
			}
			return typeName;
		}

		public static string GetSerializeTypeName(Type type, bool xmlName = false)
		{
			if (!xmlName && IsSubclassOfDictionary(type) && !IsDictionary(type))
			{
				return string.Format("{0}_{1}_{2}", TypeUtil.GetGenericTypeName(type),
					GetSerializeTypeName(GetDictionaryKeyType(type)),
					GetSerializeTypeName(GetDictionaryValueType(type)));
			}
			else if (IsDictionary(type) || IsSubclassOfDictionary(type))
			{
				return GetSerializeTypeName(GetDictionaryValueType(type), xmlName) + "s";
			}
			else if (TypeUtil.IsArray(type))
			{
				return GetSerializeTypeName(GetArrayValueType(type), xmlName) + "s";
			}
			else if (TypeUtil.IsList(type) || TypeUtil.IsSubclassOfList(type))
			{
				return GetSerializeTypeName(GetListValueType(type), xmlName) + "s";
			}
			else if (xmlName && serializeTypeNames.ContainsKey(type))
			{
				return serializeTypeNames[type];
			}
			else
			{
				return notValidChar.Replace(TypeUtil.GetTypeNameWithNest(type), "_").TrimEnd('_');
			}
		}

		public static string GetGenericTypeName(Type type)
		{
			if (!IsGenericType(type))
				throw new ArgumentException("type is not generic");
			return type.Name.Substring(0, type.Name.IndexOf("`"));
		}

		public static string GetCSharpFullTypeName(Type type, bool removeNonSystemNamespace = false)
		{
			StringBuilder sb = new StringBuilder();
			char[] fullName = type.ToString().ToCharArray();
			ParseCSharpFullTypeName(fullName, 0, sb, removeNonSystemNamespace);
			return sb.ToString();
		}

		private static int ParseCSharpFullTypeName(char[] fullName, int index, StringBuilder builder, bool removeNonSystemNamespace = false)
		{
			index = ParseCSharpName(fullName, index, builder, removeNonSystemNamespace);

			if (index < fullName.Length && fullName[index] == '+')
			{
				if (!removeNonSystemNamespace) builder.Append('.');
				index = ParseCSharpName(fullName, ++index, builder, removeNonSystemNamespace);
			}

			if (index < fullName.Length && fullName[index] == '`')
			{
				int count = 0;
				index++;
				while (fullName[index] >= '0' && fullName[index] <= '9')
				{
					count = count * 10 + fullName[index] - '0';
					index++;
				}

				string nested = "";
				if (fullName[index] == '+')
				{
					StringBuilder nestBuilder = new StringBuilder();
					nestBuilder.Append('.');
					index = ParseCSharpName(fullName, index + 1, nestBuilder, removeNonSystemNamespace);
					nested = nestBuilder.ToString();
				}

				index++;
				builder.Append('<');
				for (int i = 0; i < count; i++)
				{
					if (fullName[index] == '[')
					{
						index++;
						index = ParseCSharpFullTypeName(fullName, index, builder, removeNonSystemNamespace);
						index++;
					}
					else
					{
						index = ParseCSharpFullTypeName(fullName, index, builder, removeNonSystemNamespace);
					}
					if (i < count - 1)
					{
						builder.Append(',');
						index++;
					}
				}
				builder.Append('>');
				index++;

				builder.Append(nested);
			}

			while (index < fullName.Length && fullName[index] == '[')
			{
				builder.Append(fullName[index]);
				index++;

				while (index < fullName.Length && fullName[index] == ',')
				{
					builder.Append(fullName[index]);
					index++;
				}

				if (index < fullName.Length && fullName[index] == ']')
				{
					builder.Append(fullName[index]);
					index++;
				}
				else
				{
					break;
				}
			}

			return index;
		}

		private static int ParseCSharpName(char[] fullName, int index, StringBuilder builder, bool removeNonSystemNamespace)
		{
			StringBuilder sb = new StringBuilder();
			while (index < fullName.Length)
			{
				char ch = fullName[index];
				if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= '0' && ch <= '9' || ch == '.')
				{
					sb.Append(ch);
					index++;
				}
				else
				{
					//移除掉嵌套类名前缀
					if (ch == '+' && removeNonSystemNamespace)
						sb.Remove(0, sb.Length);
					break;
				}
			}

			string s = ReplacePrimitiveTypeName(sb.ToString());
			//移除掉嵌套类名前缀
			if (removeNonSystemNamespace) s = s.Split('.').LastOrDefault();

			builder.Append(s);
			return index;
		}

		public static string GetTypeNameWithNest(Type type)
		{
			string baseName;
			if (type.IsNested)
			{
				int start = type.FullName.LastIndexOf(".") + 1;
				int end = type.FullName.IndexOf("+");
				var parent = type.FullName.Substring(start, end - start);
				baseName = parent + "." + GetGenericTypeNameWithParams(type);
			}
			else
			{
				baseName = GetGenericTypeNameWithParams(type);
			}
			return baseName.Replace('+', '.');
		}

		public static string GetGenericTypeNameWithParams(Type t, bool full = false)
		{
			if (t.IsArray)
			{
				return GetGenericTypeNameWithParams(t.GetElementType(), full) + "[]";
			}
			if (!IsGenericType(t))
			{
				return full ? t.FullName.Replace('+', '.') : t.Name;
			}
			string genericTypeName = (full ? t.GetGenericTypeDefinition().Namespace + "." : "") + t.GetGenericTypeDefinition().Name;
			genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
			string genericArgs = string.Join(",",
				t.GetGenericArguments().Select(ta => GetGenericTypeNameWithParams(ta, full)).ToArray());
			return string.Format("{0}<{1}>", genericTypeName, genericArgs).Replace('+', '.'); ;
		}

		public static Type GetCollectionValueType(Type type)
		{
			if (IsSubclassOfDictionary(type))
				return GetDictionaryValueType(type);
			if (IsSubclassOfList(type))
				return GetListValueType(type);
			if (IsArray(type))
				return GetArrayValueType(type);
			if (IsList(type))
				return GetListValueType(type);
			return null;
		}

		public static Type FindGenerticAncestor(Type type, Type wantedType)
		{
			for (Type ancestor = type; ancestor != null; ancestor = BaseType(ancestor))
			{
				if (IsGenericType(ancestor) && ancestor.GetGenericTypeDefinition() == wantedType)
					return ancestor;
			}
			return null;
		}

		public static Type MakeDictionaryType(Type keyType, Type valueType)
		{
			Type generic = typeof(Dictionary<,>);
			return generic.MakeGenericType(keyType, valueType);
		}

		public static void UpdateDictionary(IDictionary dict, ICollection container, string dataName, MemberInfo keyField, string path = "", bool warnOnZeroId = true)
		{
			if (dict != null && container != null && keyField != null)
			{
				foreach (object elem in container)
				{
					if (elem != null)
					{
						object key = null;
						if (keyField is FieldInfo)
							key = (keyField as FieldInfo).GetValue(elem);
						else if (keyField is PropertyInfo)
							key = (keyField as PropertyInfo).GetValue(elem, null);
						if (warnOnZeroId)
						{
							if (key is int && ((int)key) == 0)
							{
								var err = string.Format("表{0}.xml中发现ID为0，请策划修改配表！", dataName);
								PrintLog(err, ConsoleColor.Yellow);
							}
						}
						else if (key == null)
						{
							var err = string.Format("表{0}.xml中发现ID为null，请策划修改配表！", dataName);
							PrintLog(err, ConsoleColor.Yellow);
						}
						else if (dict[key] != null)
						{
							throw new NotImplementedException();
							/*if (AppConfig.validation.validate_config)
							{
								IDAttribute[] attrs = (IDAttribute[])keyField.GetCustomAttributes(typeof(IDAttribute), true);
								foreach (IDAttribute attribute in attrs)
								{
									ValidationLog.Warn(attribute.LastErrorStr(), path, key.ToString());
								}
							}
							if (ConfigSerializer.WarnOnDuplicatedID)
							{
								var err = string.Format("表{0}.xml中发现重复ID:{1}，请策划修改配表！", field.Name, key);
								PrintLog(err, ConsoleColor.Red);
							}*/
						}
						dict[key] = elem;
					}
				}
			}
		}

		/*public static void UpdateDictionary(IObjectDictionary dict, ICollection container, FieldInfo field, FieldInfo keyField, string path = "")
		{
			if (dict != null && container != null && keyField != null)
			{
				foreach (object elem in container)
				{
					if (elem != null)
					{
						var key = keyField.GetValue(elem);
						if (ConfigSerializer.WarnOnZeroID)
						{
							if (keyField.Name == "Id" && key is int && ((int)key) == 0)
							{
								var err = string.Format("表{0}.xml中发现ID为0，请策划修改配表！", field.Name);
								PrintLog(err, ConsoleColor.Yellow);
							}
						}
						if (dict.ContainsObjectKey(key))
						{
							if (AppConfig.validation.validate_config)
							{
								IDAttribute[] attrs = (IDAttribute[])keyField.GetCustomAttributes(typeof(IDAttribute), true);
								foreach (IDAttribute attribute in attrs)
								{
									ValidationLog.Warn(attribute.LastErrorStr(), path, key.ToString());
								}
							}
							if (ConfigSerializer.WarnOnDuplicatedID)
							{
								var err = string.Format("表{0}.xml中发现重复ID:{1}，请策划修改配表！", field.Name, key);
								PrintLog(err, ConsoleColor.Red);
							}
						}
						dict.SetElem(key, elem);
					}
				}
			}
		}*/

		private static void PrintLog(string log, ConsoleColor col)
		{
			Console.WriteLine(log);
		}

		public static object InvokeDefaultConstructor(Type type)
		{
			ConstructorInfo defaultConstructor = type.GetConstructor(EmptyTypeList);
			if (defaultConstructor != null)
				return defaultConstructor.Invoke(null);
			return null;
		}

		public static object InvokeArrayConstructor(Type type, int count)
		{
			ConstructorInfo arrayConstructor = type.GetConstructor(ParamInt);
			if (arrayConstructor != null)
				return arrayConstructor.Invoke(new object[] { count });
			return null;
		}

		public static object InvokeCopyConstructor(Type type, IDictionary<int, object> src)
		{
			ConstructorInfo copyConstructor = type.GetConstructor(ParamDict);
			if (copyConstructor != null)
				return copyConstructor.Invoke(new object[] { src });
			return null;
		}


		public struct FieldType
		{
			public Type type;
			public string field;

			public FieldType(Type type, string field)
			{
				this.type = type;
				this.field = field;
			}
		}
		public static HashSet<FieldType> TraversalGetRelatedFieldTypes(Type rootType)
		{
			var visited = new HashSet<Type>();
			var queueToVisit = new Queue<Type>();
			var result = new HashSet<FieldType>();
			queueToVisit.Enqueue(rootType);

			while (queueToVisit.Count > 0)
			{
				Type curType = queueToVisit.Dequeue();
				if (curType == null)
					continue;

				visited.Add(curType);
				if (!IsEnum(curType))
				{
					foreach (FieldInfo field in curType.GetFields())
					{
						if (!visited.Contains(field.FieldType))
							queueToVisit.Enqueue(field.FieldType);
						result.Add(new FieldType(field.FieldType, field.Name));
					}
				}
				if (curType.HasElementType)
				{
					var elemType = curType.GetElementType();
					if (elemType != null && !visited.Contains(elemType))
						queueToVisit.Enqueue(elemType);
					result.Add(new FieldType(elemType, null));
				}
				if (IsGenericType(curType))
				{
					foreach (Type paramType in curType.GetGenericArguments())
					{
						if (!visited.Contains(paramType))
							queueToVisit.Enqueue(paramType);
						result.Add(new FieldType(paramType, null));
					}
				}
			}

			return result;
		}

		public static string GetXmlTypeName(Type type)
		{
			if (type.IsDefined(typeof(XmlTypeAttribute), true))
			{
				var attr = GetCustomAttribute<XmlTypeAttribute>(type, true);
				return attr.TypeName;
			}
			else
			{
				return type.Name;
			}
		}

		public static List<FieldInfo> GetTagedFields(Type type, Type attribute, BindingFlags customFlags = 0)
		{
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | customFlags);

			List<FieldInfo> ret = new List<FieldInfo>();
			foreach (FieldInfo field in fields)
			{
				if (field.IsDefined(attribute, true))
				{
					ret.Add(field);
				}
			}
			return ret;
		}

		public static List<FieldInfo> GetPublicInstanceFieldsExcept(Type type, Type[] attributeExclude)
		{
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			List<FieldInfo> ret = new List<FieldInfo>();
			foreach (FieldInfo field in fields)
			{
				if (!attributeExclude.Any(attr => field.IsDefined(attr, true)))
					ret.Add(field);
			}
			return ret;
		}

		public static List<FieldAndAttribute<TAttribute>> GetFieldAndAttribute<TAttribute>(Type type, BindingFlags customFlags = 0) where TAttribute : Attribute
		{
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | customFlags);

			Type typeAttr = typeof(TAttribute);
			List<FieldAndAttribute<TAttribute>> ret = new List<FieldAndAttribute<TAttribute>>();
			foreach (FieldInfo field in fields)
			{
				if (field.IsDefined(typeAttr, true))
				{
					ret.Add(new FieldAndAttribute<TAttribute>()
					{
						fieldType = field.FieldType,
						fieldName = field.Name,
						attribute = GetCustomAttribute<TAttribute>(field, true)
					});
				}
			}
			return ret;
		}

		public static IEnumerable<Type> GetXmlIncludeTypes(Type baseType)
		{
			return baseType.GetCustomAttributes(typeof(XmlIncludeAttribute), false)
				.Cast<XmlIncludeAttribute>()
				.Select(o => o.Type)
				.Union(XmlUtil.GetDerivedClasses(baseType));
		}

		public static void ResizeGenericList(IList list, int newCount)
		{
			Global.Assert(newCount >= 0);
			Global.Assert(list != null);
			Global.Assert(IsSubclassOfList(list.GetType()));
			Type elemType = GetListValueType(list.GetType());

			while (list.Count > newCount)
			{
				list.RemoveAt(list.Count - 1);
			}

			while (list.Count < newCount)
			{
				list.Add(CreateInstance(elemType));
			}
		}

		public static Array ResizeArray(Array array, int newCount)
		{
			Global.Assert(newCount >= 0);
			Global.Assert(array != null);

			Type elemType = array.GetType().GetElementType();
			Global.Assert(elemType != null);

			Array resized = Array.CreateInstance(elemType, newCount);
			for (int i = 0; i < resized.Length; ++i)
			{
				if (i < array.Length)
				{
					resized.SetValue(array.GetValue(i), i);
				}
				else
				{
					resized.SetValue(CreateInstance(elemType), i);
				}
			}
			return resized;
		}

		public static object[] BuildDefaultParamList(MethodInfo method)
		{
			ParameterInfo[] paramInfoList = method.GetParameters();
			var paramList = new object[paramInfoList.Length];
			for (int i = 0; i < paramInfoList.Length; ++i)
			{
				var paramInfo = paramInfoList[i];
				if (paramInfo.IsOptional)
				{
					paramList[i] = paramInfo.DefaultValue;
				}
				else
				{
					try
					{
						paramList[i] = CreateInstance(paramInfo.ParameterType);
					}
					catch (Exception)
					{
						paramList[i] = null;
					}
				}
			}
			return paramList;
		}

		public static object CreateInstance(Type type)
		{
			if (type == typeof(string))
				return "";
			else
				return Activator.CreateInstance(type);
		}

		public static T GetAttribute<T>(Type type)
		{
			return type.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
		}

		public static T GetAttribute<T>(FieldInfo info)
			where T : class
		{
			return info.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
		}

		// 由于mono下 nested class 的 .NameSpace属性有问题，所以整了这个 workaround
		public static string GetNameSpace(Type o)
		{
			var nameSpace = o.Namespace;
			if (!string.IsNullOrEmpty(nameSpace))
				return nameSpace;

			while (o.HasElementType)
			{
				o = o.GetElementType();
			}

			while (o.IsNested)
			{
				o = o.DeclaringType;
			}
			return o.Namespace;
		}

		public static string[] GetNameSpacesWithGeneric(Type o)
		{
			var ns = new List<string>();
			string name = GetNameSpace(o);
			if (!string.IsNullOrEmpty(name)) ns.Add(name);

			foreach (var type in o.GetGenericArguments())
			{
				name = GetNameSpace(type);
				if (!string.IsNullOrEmpty(name) && !ns.Contains(name)) ns.Add(name);
			}
			return ns.ToArray();
		}

		public static string GetEnumComment(Enum value)
		{
			return GetEnumComment(value.GetType(), value);
		}

		public static string GetEnumComment(Type enumType, Enum value)
		{
			var fieldInfo = enumType.GetField(value.ToString(), BindingFlags.Static | BindingFlags.Public);
			if (fieldInfo != null)
			{
				CommentAttribute attr = fieldInfo.GetCustomAttributes(typeof(CommentAttribute), false).FirstOrDefault() as CommentAttribute;
				return attr != null ? attr.Comment : null;
			}
			return null;
		}

		///返回[枚举项1](连接符)[枚举项2](连接符)[枚举项2]这样的格式
		public static string GetMaskEnumComment(Enum value, string connecter)
		{
			int maskValue = (int)(object)value;

			StringBuilder sb = new StringBuilder("");
			FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
			bool isEveryThing = true;
			for (int i = 0; i < fields.Length; i++)
			{
				CommentAttribute attr = fields[i].GetCustomAttributes(typeof(CommentAttribute), false).FirstOrDefault() as CommentAttribute;
				string text = attr != null ? attr.Comment : fields[i].Name;
				int itemValue = (int)fields[i].GetValue(null);
				if ((itemValue & maskValue) != 0)
				{
					if (sb.Length > 0)
					{
						sb.Append(connecter);
					}
					sb.Append(text);
				}
				else
				{
					isEveryThing = false;
				}
			}

			if (isEveryThing)
			{
				return "EveryThing";
			}
			return sb.ToString();
		}

		public static T GetStaticField<T>(Type type, string name)
		{
			if (type == null)
				return default(T);

			FieldInfo field = type.GetField(name);
			if (field != null && typeof(T).IsAssignableFrom(field.FieldType))
				return (T)field.GetValue(null);
			else
				return default(T);
		}

		public static bool IsSubclassOfTypeName(Type type, string typeName)
		{
			while (type != null && type != typeof(object))
			{
				if (type.Name == typeName)
					return true;

				type = type.BaseType;
			}
			return false;
		}

		public static bool IsAbstract(Type type)
		{
			return type.IsAbstract;
		}

		public static bool IsAnsiClass(Type type)
		{
			return type.IsAnsiClass;
		}

		public static bool IsArray(Type type)
		{
			return type.IsArray;
		}

		public static bool IsAutoClass(Type type)
		{
			return type.IsAutoClass;
		}

		public static bool IsAutoLayout(Type type)
		{
			return type.IsAutoLayout;
		}

		public static bool IsByRef(Type type)
		{
			return type.IsByRef;
		}

		public static bool IsClass(Type type)
		{
			return type.IsClass;
		}

		public static bool IsEnum(Type type)
		{
			return type.IsEnum;
		}

		public static bool IsExplicitLayout(Type type)
		{
			return type.IsExplicitLayout;
		}

		public static bool IsGenericParameter(Type type)
		{
			return type.IsGenericParameter;
		}

		public static bool IsGenericType(Type type)
		{
			return type.IsGenericType;

		}

		public static bool IsGenericTypeDefinition(Type type)
		{
			return type.IsGenericTypeDefinition;
		}

		public static bool IsImport(Type type)
		{
			return type.IsImport;

		}

		public static bool IsInterface(Type type)
		{
			return type.IsInterface;
		}

		public static bool IsLayoutSequential(Type type)
		{
			return type.IsLayoutSequential;
		}

		public static bool IsMarshalByRef(Type type)
		{
			return type.IsMarshalByRef;
		}

		public static bool IsNested(Type type)
		{
			return type.IsNested;
		}

		public static bool IsNestedAssembly(Type type)
		{
			return type.IsNestedAssembly;
		}

		public static bool IsNestedFamANDAssem(Type type)
		{
			return type.IsNestedFamANDAssem;
		}

		public static bool IsNestedFamily(Type type)
		{
			return type.IsNestedFamily;
		}

		public static bool IsNestedFamORAssem(Type type)
		{
			return type.IsNestedFamORAssem;
		}

		public static bool IsNestedPrivate(Type type)
		{
			return type.IsNestedPrivate;
		}

		public static bool IsNestedPublic(Type type)
		{
			return type.IsNestedPublic;
		}

		public static bool IsNotPublic(Type type)
		{
			return type.IsNotPublic;
		}

		public static bool IsPointer(Type type)
		{
			return type.IsPointer;
		}

		public static bool IsPrimitive(Type type)
		{
			return type.IsPrimitive;
		}

		public static bool IsPublic(Type type)
		{
			return type.IsPublic;
		}

		public static bool IsSealed(Type type)
		{
			return type.IsSealed;
		}

		public static bool IsSerializable(Type type)
		{
			return type.IsSerializable;
		}

		public static bool IsSpecialName(Type type)
		{
			return type.IsSpecialName;
		}

		public static bool IsUnicodeClass(Type type)
		{
			return type.IsUnicodeClass;
		}

		public static bool IsValueType(Type type)
		{
			return type.IsValueType;
		}

		public static bool IsVisible(Type type)
		{
			return type.IsVisible;
		}

		public static T GetCustomAttribute<T>(MemberInfo member, bool inherit) where T : Attribute
		{
			var customAttributes = member.GetCustomAttributes(typeof(T), inherit);
			return customAttributes.Length > 0 ? (T)customAttributes[0] : null;
		}

		public static T GetCustomAttribute<T>(Type type, bool inherit) where T : Attribute
		{
			var customAttributes = type.GetCustomAttributes(typeof(T), inherit);
			return customAttributes.Length > 0 ? (T)customAttributes[0] : null;
		}

		//是否实现某个接口
		//type: 接口类型
		public static bool HasImplInterface(Type type, Type inte)
		{
			return type.GetInterface(inte.Name) != null;
		}

		public static Type BaseType(Type type)
		{
			return type.BaseType;
		}

		public static List<FieldInfo> GetSerializedFields(Type type)
		{
			var attributes = new[]
			{
				typeof(NonSerializedAttribute)
			};
			var fields = TypeUtil.GetPublicInstanceFieldsExcept(type, attributes);
			fields.RemoveAll(o => TypeUtil.IsDelegation(o.FieldType));
			fields.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
			return fields;
		}

		public static object FindConfigKeyValue(Type t, object v)
		{
			var dictionaryAttri = TypeUtil.GetCustomAttribute<DictionaryConfigAttribute>(t, false);
			if (dictionaryAttri == null)
			{
				return null;
			}
			var keyName = dictionaryAttri.Key;
			var field = t.GetField(keyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
			if (field == null)
			{
				var property = t.GetProperty(keyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (property == null)
				{
					return null;
				}
				return property.GetValue(v, null);
			}
			else
			{
				return field.GetValue(v);
			}
		}
	}
}

