using Kernel.Config;
using Kernel.Lang.Attribute;

namespace Config
{
	[DictionaryConfig(Name = "heroskins", ExportPolicy = ConfigExportPolicy.EXPORT_TO_BOTH, LoadAll = true, Key = "Id")]
	public class ConfHeroSkin
	{
		[Id]
		public int Id;

		[RefId(typeof(ConfHero))]
		public int HeroId;

		public string Desc;
	}
}
