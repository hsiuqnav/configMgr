using Kernel;
using Kernel.Config;
using Kernel.Engine;
using Kernel.Lang.Attribute;
using System;
using System.Text;

namespace Config
{
	[DictionaryConfig(Name = "heroes", Key = "Id")]
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

		[Comment("可选颜色")]
		public Color[] AvailableColors;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Id : {0}, Name : {1}, Desc : {2}, Hp : {3}, Attack : {4}, BirthDay : {5}", Id, Name, Locale.L(Desc), Hp, Attack, BirthDay);
			for(var i = 0; i < AvailableColors.Length; ++i)
			{
				sb.AppendFormat(" AvailableColor{0} : {1}", i + 1, AvailableColors[i].ToString());
			}
			return sb.ToString();
		}
	}
}
