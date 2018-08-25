using Kernel.Config;

namespace Config
{
	[ConstConfig]
	public class ConfSettingConst
	{
		public bool UseAA;
		public string Locale;
		public string Version;

		public override string ToString()
		{
			return string.Format("ConfSettings, UseAA : {0}, Locale : {1}, Version : {2}", UseAA, Locale, Version);
		}
	}
}
