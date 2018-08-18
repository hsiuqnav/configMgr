using GeneratedCode;
using Kernel;
using Kernel.Config;
using Kernel.FSM;
using Kernel.Game;
using Kernel.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Alche.Runtime
{
	public class GameCodeGenState : GameFSMState
	{
		private readonly string targetFolder;
		private readonly string[] targetNamespaces;
		public GameCodeGenState(Game content, string targetFolder, string[] targetNamespaces) : base(content)
		{
			this.targetFolder = targetFolder;
			this.targetNamespaces = targetNamespaces;
		}

		protected override void OnEnter(Event e, FiniteStateMachine<Game, float>.State lastState)
		{
			ConfigManager.Instance.SetSerializer(new ConfigSerializer());
			ManagerMan.Instance.RegisterManager(PlatformManager.Instance);
			ManagerMan.Instance.RegisterManager(PathManager.Instance);
			ManagerMan.Instance.RegisterManager(WorksManager.Instance);
			ManagerMan.Instance.InitAllManagers();
			base.OnEnter(e, lastState);
		}

		protected override GameFSM.State DoTick(float deltaTime)
		{
			if (!IsFinished)
			{
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
				return new GameQuitState(Content);
			}
			return base.DoTick(deltaTime);
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
