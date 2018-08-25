using Kernel;
using Kernel.Config;
using Kernel.Lang.Attribute;
using System;

namespace Config
{
	[DictionaryConfig(Name = "heroes", Key = "Id")]
	[XmlDerivedFrom(typeof(ConfCharacter))]
	public class ConfHero : ConfCharacter
	{
		[Comment("英雄血量")]
		public float Hp;

		[Comment("英雄攻击力")]
		public float Attack;

		[Comment("生日")]
		public DateTime BirthDay;

		[Comment("英雄品质")]
		public QualificationType Quality;

		public override string ToString()
		{
			return string.Format("Id : {0}, Name : {1}, Desc : {2}, Hp : {3}, Attack : {4}, BirthDay : {5}", Id, Name, Locale.L(Desc), Hp, Attack, BirthDay);
		}
	}
}
