using System;
using System.Collections.Generic;
using System.IO;
using Kernel;
using Kernel.Config;

namespace GeneratedCode
{
	public partial class ConfigSerializer : ConfigSerializerBase
	{
		private static readonly ConfigFieldInfo[] fields =
		{
			new ConfigFieldInfo("heroes", typeof(System.Collections.Generic.Dictionary<int,Config.ConfHero>), typeof(Config.ConfHero), ConfigFieldInfo.Mode.KEY_VALUE, true, "Id", false),
			new ConfigFieldInfo("heroskins", typeof(System.Collections.Generic.Dictionary<int,Config.ConfHeroSkin>), typeof(Config.ConfHeroSkin), ConfigFieldInfo.Mode.KEY_VALUE, true, "Id", false),
			new ConfigFieldInfo("actions", typeof(System.Collections.Generic.Dictionary<string,Config.ConfAction>), typeof(Config.ConfAction), ConfigFieldInfo.Mode.KEY_VALUE, true, "ActionName", false),
		};
		
		public override ConfigFieldInfo[] Fields { get { return fields; } }
		
		public override string Hash { get { return "2b4047b87596d5c04cea09254656dee0"; } }
		
		private static readonly Dictionary<Type, IConfigSerializer> serializers = new Dictionary<Type, IConfigSerializer>
		{
			{ typeof(Config.ConfAction), new ConfActionSerializer() },
			{ typeof(Config.ConfHero), new ConfHeroSerializer() },
			{ typeof(Config.ConfHeroSkin), new ConfHeroSkinSerializer() },
			{ typeof(Kernel.Config.ConfigHeader), new ConfigHeaderSerializer() },
			{ typeof(Kernel.Config.FieldLayout), new Poli_FieldLayoutSerializer() },
			{ typeof(Kernel.Config.FieldLayout[]), new ArrayPoli_FieldLayoutSerializer() },
			{ typeof(Kernel.Config.FieldLayoutOfConst), new FieldLayoutOfConstSerializer() },
			{ typeof(Kernel.Config.FieldLayoutOfIntKey), new FieldLayoutOfIntKeySerializer() },
			{ typeof(Kernel.Config.FieldLayoutOfStringKey), new FieldLayoutOfStringKeySerializer() },
			{ typeof(System.Collections.Generic.Dictionary<System.Int32,Config.ConfHero>), new Dictionary_Int32_ConfHeroSerializer() },
			{ typeof(System.Collections.Generic.Dictionary<System.Int32,Config.ConfHeroSkin>), new Dictionary_Int32_ConfHeroSkinSerializer() },
			{ typeof(System.Collections.Generic.Dictionary<System.Int32,System.Int32>), new Dictionary_Int32_Int32Serializer() },
			{ typeof(System.Collections.Generic.Dictionary<System.String,Config.ConfAction>), new Dictionary_String_ConfActionSerializer() },
			{ typeof(System.Collections.Generic.Dictionary<System.String,System.Int32>), new Dictionary_String_Int32Serializer() },
			{ typeof(System.DateTime), new DateTimeSerializer() },
			{ typeof(System.String), new StringSerializer() },
		};
		
		public override Dictionary<Type, IConfigSerializer> Serializers { get { return serializers; } }
	}
}
