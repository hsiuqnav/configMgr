using Kernel.Config;
using Kernel.Lang.Attribute;

namespace Config
{
	[DictionaryConfig(Name = "actions", ExportPolicy = ConfigExportPolicy.EXPORT_TO_BOTH, LoadAll = true, Key = "ActionName")]
	public class ConfAction
	{
		[Id]
		public string ActionName;
		public float ActionLength;

		public override string ToString()
		{
			return string.Format("ActionName : {0}, ActionLength : {1}", ActionName, ActionLength);
		}
	}
}
