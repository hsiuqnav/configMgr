using Kernel.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Kernel
{
    public class XmlUtil 
    {
        // DefaultNamespace 比C#自带的少一个，只有一个namespace xsi。避免export时两个namespace有概率顺序不一致导致的svn提交问题。
        // xsi:type 用于继承，是必须的。
        // 删除的namespace为： xsd="http://www.w3.org/2001/XMLSchema
        public static readonly XmlSerializerNamespaces DefaultNamespace = new XmlSerializerNamespaces();

        public static bool ValidateSchema = false;

        private static readonly Dictionary<string, XmlSerializer> cachedFileSerialzier = new Dictionary<string, XmlSerializer>();
        private static readonly Dictionary<Type, XmlSerializer> cachedStringSerialzier = new Dictionary<Type, XmlSerializer>();
        private static readonly StringWriter stringWriter = new StringWriter(new StringBuilder(1024));

        private static List<Type> derivedClass;
        private static Dictionary<Type, HashSet<Type>> baseClass;

        static XmlUtil()
        {
            DefaultNamespace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        }

        public static void InitDerivedClassList(params Type[] types)
        {
#if CODE_GEN || UNITY_EDITOR
            HashSet<string> assemblies = new HashSet<string>();
            List<Type> allTypes = new List<Type>();
            foreach (var t in types)
            {
                if (!assemblies.Contains(t.Assembly.FullName))
                {
                    allTypes.AddRange(t.Assembly.GetTypes());
                    assemblies.Add(t.Assembly.FullName);
                }
            }

            foreach (var t in allTypes)
            {
                var attr = TypeUtil.GetAttribute<XmlDerivedFromAttribute>(t);
                if (attr != null)
                {
                    AddCustomDerivedClass(t, attr.BaseType);
                }
            }
#endif
        }

        public static void AddCustomDerivedClass(Type deriveType, Type baseType)
        {
            if (derivedClass == null) derivedClass = new List<Type>();
            if (baseClass == null) baseClass = new Dictionary<Type, HashSet<Type>>();

            if (!derivedClass.Contains(deriveType))
            {
                derivedClass.Add(deriveType);
                if (!baseClass.ContainsKey(baseType))
                {
                    baseClass.Add(baseType, new HashSet<Type>());
                }
                var derived = baseClass[baseType];
                derived.Add(deriveType);
            }
        }

        public static bool IsPolymorphicClass(Type type)
        {
            return TypeUtil.GetAttribute<XmlIncludeAttribute>(type) != null || baseClass != null && baseClass.ContainsKey(type);
        }

        public static IEnumerable<Type> GetDerivedClasses(Type type)
        {
            return baseClass != null && baseClass.ContainsKey(type) ? (IEnumerable<Type>)baseClass[type] : new Type[0];
        }

        // 从path读取type
        public static object Read(string path, Type type, string root, bool useMultiThread = false, XmlAttributeOverrides overrides = null)
        {
            if (!PlatformManager.Instance.FileExists(path))
                return null;

            XmlSerializer parser = GetFileParser(path, type, root, useMultiThread, overrides);
            try
            {
                if (ValidateSchema)
                {
                    CheckWithSchema(path);
                }
                using (var reader = new StringReader(PlatformManager.Instance.ReadAllText(path)))
                {
                    var result = parser.Deserialize(reader);
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("XML文件读取失败！ 文件{0} [试图读取的数据类型是{1}]。\n详细内部错误：{2}", path, type.Name, e));
            }
        }

        public static object ReadFromBinary(byte[] content, Type type, string root)
        {
            XmlSerializer parser = GetFileParser("", type, root);
            try
            {
                using (MemoryStream stream = new MemoryStream(content))
                {
                    var result = parser.Deserialize(stream);
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("XML 内容读取失败！ [试图读取的数据类型是{0}]。\n详细内部错误：{1}", type.Name, e));
            }
        }

        // 将type序列化到path
        public static bool Write(string path, object data, Type type, string root = null, XmlAttributeOverrides overrides = null)
        {
            XmlSerializer parser = GetFileParser(path, type, root, false, overrides);

            try
            {
                PlatformManager.Instance.CreateParentDirectoryIfNeed(path);
                PlatformManager.Instance.RemoveReadonlyAttribute(path);

                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.NewLine = "\r\n";
                    parser.Serialize(writer, data, DefaultNamespace);

                    File.WriteAllBytes(path, stream.ToArray());
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Fatal("XML文件保存失败！ 文件{0}。\n详细内部错误：{1}", path, e);
                return false;
            }
        }

        public static string EncodeToString(object data, Type type)
        {
            XmlSerializer serializer = GetStringParser(type);
            try
            {
                serializer.Serialize(stringWriter, data);
                return stringWriter.ToString();
            }
            finally
            {
                stringWriter.GetStringBuilder().Length = 0;
            }
        }

        //////////////////////////////////////////////////////////////////////
        // impl
        private static void CheckWithSchema(string path)
        {
            //XmlReaderSettings没有Schemas, ValidationType, ValidationFlags
            XmlReaderSettings settings = new XmlReaderSettings();

            string xsd = GetSchemaFile(Path.GetFileNameWithoutExtension(path));
            if (xsd != null)
            {
                settings.Schemas.Add(null, xsd);
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                try
                {
                    using (Stream stream = new FileStream(path,
                        FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        XmlReader reader = XmlReader.Create(stream, settings);

                        XmlDocument doc = new XmlDocument();
                        doc.Load(reader);

                        doc.Validate(ValidationCallBack);
                    }
                }
                catch (XmlSchemaValidationException e)
                {
                    Logger.Warn("配置格式错误（文件{0}，行{1}，列{2}）\t{3} ", Path.GetFileName(path), e.LineNumber, e.LinePosition, e.Message);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.ToString());
                }
            }
        }

        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            Logger.Warn(args.Message);
        }

        private static string GetSchemaFile(string name)
        {
            string xsdpath = "validator/xsd/" + name + ".xsd";
            return File.Exists(xsdpath) ? xsdpath : null;
        }

        private static XmlSerializer GetFileParser(string path, Type type, string root, bool useMultiThread = false, XmlAttributeOverrides overrides = null)
        {
            if (useMultiThread)
                return string.IsNullOrEmpty(root) ? new XmlSerializer(type) : new XmlSerializer(type, overrides, derivedClass != null ? derivedClass.ToArray() : new Type[0], new XmlRootAttribute(root), null);

            string key = string.Format("{0}[{1}]{2}", path, type, root);
            if (!cachedFileSerialzier.ContainsKey(key))
            {
                XmlSerializer serializer = string.IsNullOrEmpty(root) ? new XmlSerializer(type) : new XmlSerializer(type, overrides, derivedClass != null ? derivedClass.ToArray() : new Type[0], new XmlRootAttribute(root), null);
                cachedFileSerialzier[key] = serializer;
            }
            return cachedFileSerialzier[key];
        }

        private static XmlSerializer GetStringParser(Type type)
        {
            if (!cachedStringSerialzier.ContainsKey(type))
            {
                cachedStringSerialzier[type] = new XmlSerializer(type);
            }
            return cachedStringSerialzier[type];
        }
    }
}
