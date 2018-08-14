using GeneratedCode;
using Kernel;
using Kernel.Config;
using Kernel.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Alche.Runtime
{
    public class GameCodeGenApp
    {
        private readonly string targetFolder;
        private readonly string[] targetNamespaces;

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                System.Console.WriteLine("Usage: codegen <csfolder> <luafolder> <il2luafolder> [namespace1;namespace2]");
                return;
            }
            GameCodeGenApp app = new GameCodeGenApp(args[0], args.Length > 1 ? args[1] : null);
            app.Start();
        }

        public GameCodeGenApp(string targetFolder, string targetNamespaces)
        {
            this.targetFolder = targetFolder;
            this.targetNamespaces = targetNamespaces != null ? targetNamespaces.Trim('"').Split(';') : null;
        }

        public void Start()
        {
            ConfigManager.Instance.SetSerializer(new ConfigSerializer());
            List<Type> allTypes = GetType().Assembly.GetTypes().ToList();
            if (typeof(SerializationGenerator).Assembly != GetType().Assembly)
            {
                allTypes.AddRange(typeof(SerializationGenerator).Assembly.GetTypes());
            }

            var configTypes = GenerateCSCode(allTypes);
            ExportEnumLocale(allTypes);
#if CODE_GEN && !DISABLE_EXCEL_BUILDER
            ConfigExcelBuilder builder = new ConfigExcelBuilder();
            builder.Clear();
            builder.ExportExamples(configTypes);
            builder.ExportEnums();
#endif
        }

        private IEnumerable<KeyValuePair<Type, ConfigAttribute>> GenerateCSCode(List<Type> allTypes)
        {
            SerializationGenerator gen = new SerializationGenerator();

            Dictionary<Type, ConfigAttribute> configs = new Dictionary<Type, ConfigAttribute>();
            for (int i = 0; i < allTypes.Count; i++)
            {
                ConfigAttribute attribute = null;
                Type type = allTypes[i];
                if ((type.Namespace == null || !type.Namespace.StartsWith("GeneratedCode")) &&
                    (attribute = TypeUtil.GetAttribute<ConfigAttribute>(type)) != null && attribute.ExportToCS && !type.IsEnum
                    && IsTargetNamespace(type.Namespace))
                {
                    type = attribute.GetConfigType(type);
                    configs.Add(type, attribute);
                    gen.RegisterRoot(type);
                }
            }

            gen.Register(typeof(ConfigHeader));

            new CodeGenVisitor().Export(gen, "GeneratedCode", targetFolder);
            string md5 = new VersionVisitor().GenerateMd5(gen);
            new SerializerListVisitor().Export(gen, "GeneratedCode", md5, targetFolder);

            return configs;
        }

        private void ExportEnumLocale(List<Type> allTypes)
        {
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < allTypes.Count; i++)
            {
                ExportEnumLocaleAttribute attribute;
                if (allTypes[i].IsEnum && (attribute = TypeUtil.GetAttribute<ExportEnumLocaleAttribute>(allTypes[i])) != null
                    && attribute.LocalePrefix != null)
                {
                    var values = Enum.GetValues(allTypes[i]);
                    foreach (Enum value in values)
                    {
                        string comment = TypeUtil.GetEnumComment(allTypes[i], value);
                        if (!string.IsNullOrEmpty(comment))
                        {
                            text.AppendLine(attribute.LocalePrefix + "." + (int)(object)value);
                            text.AppendLine(comment);
                        }
                    }
                }
            }

            PlatformManager.Instance.WriteAllText(text.ToString(),
                Path.Combine(PathManager.Instance.ExternalLocaleFolder, "enum_locale.txt"));
        }

        private bool IsTargetNamespace(string ns)
        {
            if (targetNamespaces != null && targetNamespaces.Length > 0 && ns != null)
            {
                return targetNamespaces.Any(ns.StartsWith);
            }
            return true;
        }
    }
}
