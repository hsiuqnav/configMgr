using Kernel.Config;
using Kernel.Lang.Attribute;

namespace Config
{
	[DictionaryConfig(Name = "heroes", ExportPolicy = ConfigExportPolicy.EXPORT_TO_BOTH, LoadAll = true, Key = "Id")]
	public class ConfHero
	{
		[Comment("Id")]
		public int Id;

		[Comment("英雄名")]
		public string HeroName;

		[Comment("英雄描述")]
		public string HeroDesc;

		[Comment("英雄血量")]
		public float Hp;

		[Comment("英雄攻击力")]
		public float Attack;
	}
}
