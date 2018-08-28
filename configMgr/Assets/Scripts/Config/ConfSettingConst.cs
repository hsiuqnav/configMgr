using Kernel.Config;
using Kernel.Lang.Attribute;

namespace Config
{
	[ConstConfig]
	public class ConfSettingConst
	{
		[Comment("是否使用抗锯齿")]
		public bool UseAA;

		[Comment("本地化语言")]
		public string Locale;

		[Comment("版本号")]
		public string Version;

		public override string ToString()
		{
			return string.Format("ConfSettings, UseAA : {0}, Locale : {1}, Version : {2}", UseAA, Locale, Version);
		}
	}
}
