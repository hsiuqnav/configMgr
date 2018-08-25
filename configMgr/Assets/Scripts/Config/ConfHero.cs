using Kernel;
using Kernel.Config;
using Kernel.Lang.Attribute;
using System;

namespace Config
{
	[DictionaryConfig(Name = "heroes", ExportPolicy = ConfigExportPolicy.EXPORT_TO_BOTH, LoadAll = true, Key = "Id")]
	public class ConfHero
	{
		[Comment("Id")]
		[Id]
		public int Id;

		[Comment("英雄名")]
		public string HeroName;

		[Comment("英雄描述")]
		[Locale]
		public string HeroDesc;

		[Comment("英雄血量")]
		public float Hp;

		[Comment("英雄攻击力")]
		public float Attack;

		[Comment("生日")]
		public DateTime BirthDay;

		public override string ToString()
		{
			return string.Format("Id : {0}, HeroName : {1}, HeroDesc : {2}, Hp : {3}, Attack : {4}, BirthDay : {5}", Id, HeroName, Locale.L(HeroDesc), Hp, Attack, BirthDay);
		}
	}
}
