using Kernel.Lang.Attribute;

namespace Config
{
	public class ConfCharacter
	{
		[Comment("Id")]
		public int Id;

		[Comment("名字")]
		public string Name;

		[Comment("描述")]
		[Locale]
		public string Desc;

		[Comment("资源路径")]
		public string PrefabPath;
	}
}
