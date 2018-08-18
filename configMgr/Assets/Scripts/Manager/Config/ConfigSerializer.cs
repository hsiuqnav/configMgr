using Kernel.Config;

namespace GeneratedCode
{
    public partial class ConfigSerializer : ConfigSerializerBase
    {
        public override string LuaConfigTemplate
        {
            get
            {
                return "Alche.Runtime.ConfigTemplates";
            }
        }
    }
}

namespace GeneratedCode.Lua
{
    public partial class ConfigSerializer : ConfigSerializerBase
    {
        public override string LuaConfigTemplate
        {
            get
            {
                return "Alche.Runtime.ConfigTemplates";
            }
        }

        public override bool IsLuaSerializer
        {
            get
            {
                return true;
            }
        }
    }
}
