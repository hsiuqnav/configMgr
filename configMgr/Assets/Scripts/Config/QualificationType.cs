using Kernel.Config;
using Kernel.Lang.Attribute;

namespace Config
{
	[ExportEnumLocale(LocalePrefix = "QualificationType")]
	public enum QualificationType
	{
		[Comment("优秀")]
		GREEN = 1,
		[Comment("精良")]
		BLUE = 2,
		[Comment("史诗")]
		PURPLE = 3,
		[Comment("传说")]
		ORANGE = 4
	}
}
