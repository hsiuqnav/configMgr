using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Kernel.Config
{
    public class ConfigExampleBuilder
    {
        private readonly Stack<Type> visitedTypes = new Stack<Type>();

        public void WriteExampleConfig(ConfigSerializerBase serializer, string folder)
        {
            ConfigManager.Instance.Clear();
            SetConfigWithExample(serializer.Fields);
            ConfigManager.Instance.WriteAllConfig(folder);

            ConfigEnumWriter enumWriter = new ConfigEnumWriter();
            foreach (var field in serializer.Fields)
            {
                string file = string.Format("{0}/{1}.enums.txt", folder, field.Name.ToLower());
                enumWriter.CheckAndWriteAllEnum(file, field.ConfigType);
            }
        }

        private void SetConfigWithExample(ConfigFieldInfo[] fields)
        {
            foreach (ConfigFieldInfo field in fields)
            {
                var attribute = TypeUtil.GetCustomAttribute<ConfigAttribute>(field.ElemType, true);
                if (attribute != null && !attribute.ExportXmlExample) continue;

                ConfigManager.Instance.SetConfigWithExample(field.ElemType, BuildExample(field.ConfigType));
            }
        }

        private object BuildExample(Type type)
        {
            visitedTypes.Clear();
            var result = BuildExampleInstance(type);
            return result;
        }

        private object BuildExampleInstance(Type type)
        {
            if (type.IsEnum)
            {
                return BuildExampleEnum(type);
            }
            else if (type.IsPrimitive || type.IsValueType)
            {
                return BuildExamplePrimitive(type);
            }
            else if (type == typeof(string))
            {
                return " ";
            }
            else
            {
                if (visitedTypes.Count(o => o == type) >= 2)
                    return null;

                visitedTypes.Push(type);
                var result = BuildComplexType(type);
                visitedTypes.Pop();

                return result;
            }
        }

        private object BuildExamplePrimitive(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                object result = TypeUtil.InvokeDefaultConstructor(type);
                if (result == null)
                    Logger.Warn("BuildExamplePrimitiveType fail {0}", type);
                return result;
            }
        }

        private object BuildExampleEnum(Type type)
        {
            Array values = Enum.GetValues(type);
            if (values.Length > 0)
            {
                return values.GetValue(0);
            }
            else
            {
                return Activator.CreateInstance(type);
            }
        }

        private object BuildComplexType(Type type)
        {
            if (TypeUtil.IsSubclassOfDictionary(type))
            {
                return BuildExampleDictionary(type, AddExampleToDictionary);
            }
            else if (TypeUtil.IsSubclassOfList(type))
            {
                return BuildExampleList(type);
            }
            else if (type.IsArray)
            {
                return BuildExampleArray(type);
            }
            else if (TypeUtil.GetAttribute<XmlIncludeAttribute>(type) != null)
            {
                return BuildExampleClass(GetNonAbstructDerived(type).First());
            }
            else
            {
                return BuildExampleClass(type);
            }
        }

        private void AddExampleToDictionary(object dict, object key, object value)
        {
            if (dict is IDictionary)
            {
                IDictionary dictionary = dict as IDictionary;
                dictionary.Add(key, value);
            }
        }

        private object BuildExampleDictionary(Type type, Action<object, object, object> addExample)
        {
            Type keyType = TypeUtil.GetDictionaryKeyType(type);
            Type valueType = TypeUtil.GetDictionaryValueType(type);
            object result = TypeUtil.InvokeDefaultConstructor(type);

            if (result != null && addExample != null)
            {
                var examples = BuildAllXmlInclude(valueType);
                if (keyType == typeof(int))
                {
                    for (int i = 0; i < examples.Length; ++i)
                    {
                        addExample(result, i, examples[i]);
                    }
                }
                if (keyType == typeof(string))
                {
                    for (int i = 0; i < examples.Length; ++i)
                    {
                        addExample(result, "a" + i, examples[i]);
                    }
                }
                else if (keyType.IsEnum)
                {
                    Array enumValues = Enum.GetValues(keyType);
                    for (int i = 0; i < enumValues.Length && i < examples.Length; ++i)
                    {
                        addExample(result, enumValues.GetValue(i), examples[i]);
                    }
                }
            }
            else
            {
                Logger.Warn("BuildExampleIntDictioanry fail {0}", type);
            }
            return result;
        }

        private object BuildExampleClass(Type type)
        {
            if (type.IsAbstract) return null;

            object result = TypeUtil.InvokeDefaultConstructor(type);
            if (result != null)
            {
                foreach (FieldInfo field in type.GetFields())
                {
                    if (field.IsLiteral)
                        continue;
                    field.SetValue(result, BuildExampleInstance(field.FieldType));
                }
            }
            else
            {
                Logger.Warn("BuildExampleClass fail {0}", type);
            }
            return result;
        }

        private object BuildExampleArray(Type type)
        {
            Type valueType = type.GetElementType();
            var examples = BuildAllXmlInclude(valueType);
            Array result = (Array)TypeUtil.InvokeArrayConstructor(type, examples.Length);
            if (result != null)
            {
                for (int i = 0; i < examples.Length; i++)
                {
                    result.SetValue(examples[i], i);
                }
            }
            else
            {
                Logger.Warn("BuildExampleArray fail {0}", type);
            }
            return result;
        }

        private object BuildExampleList(Type type)
        {
            Type valueType = TypeUtil.GetListValueType(type);
            IList result = (IList)TypeUtil.InvokeDefaultConstructor(type);
            if (result != null)
            {
                foreach (object item in BuildAllXmlInclude(valueType))
                {
                    result.Add(item);
                }
            }
            else
            {
                Logger.Warn("BuildExampleList fail {0}", type);
            }
            return result;
        }

        private object[] BuildAllXmlInclude(Type type)
        {
            if (TypeUtil.GetAttribute<XmlIncludeAttribute>(type) == null && !XmlUtil.IsPolymorphicClass(type))
            {
                return new[] { BuildExampleInstance(type), BuildExampleInstance(type) };
            }
            else
            {
                Type[] include = GetNonAbstructDerived(type).ToArray();
                object[] examples = new object[include.Length];
                for (int index = 0; index < include.Length; index++)
                {
                    var derivedType = include[index];
                    examples[index] = BuildExampleInstance(derivedType);
                }
                return examples;
            }
        }

        private IEnumerable<Type> GetNonAbstructDerived(Type type)
        {
            var list = TypeUtil
                .GetXmlIncludeTypes(type)
                .Union(new[] { type })
                .Where(o => o != type && !o.IsAbstract)
                .OrderBy(o => o.Name).ToList();

            if (!type.IsAbstract)
                list.Insert(0, type);

            return list;
        }
    }
}
