using Kernel.Config;
using Kernel.Lang.Attribute;

namespace Config
{
	[DictionaryConfig(Name = "actions", Key = "ActionName")]
	public class ConfAction
	{
		public string ActionName;
		public float ActionLength;

		public override string ToString()
		{
			return string.Format("ActionName : {0}, ActionLength : {1}", ActionName, ActionLength);
		}
	}
}
