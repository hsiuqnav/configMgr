using Kernel.Config;
using Kernel.Lang.Attribute;

namespace Config
{
	[DictionaryConfig(Name = "heroskins", Key = "Id")]
	public class ConfHeroSkin
	{
		public int Id;

		[RefKeyAttribute(typeof(ConfHero))]
		public int HeroId;

		[Locale]
		public string Desc;

		public override string ToString()
		{
			return string.Format("ConfHeroSkin, Id {0}, HeroId {1}, Desc {2}", Id, HeroId, Desc);
		}
	}
}
