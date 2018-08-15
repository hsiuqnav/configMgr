using System;
using System.Linq;
using System.Reflection;

namespace Kernel.Config
{
    public class ConfigFieldInfo
    {
        public enum Mode
        {
            CONST,
            KEY_VALUE
        }

        public Type ConfigType;
        public string Name;
        public Type ElemType;
        public Mode FieldMode;
        public bool LoadAll;
        public string Key;
        public bool Boot;

        public MemberInfo KeyField
        {
            get
            {
                return ElemType != null ? ElemType.GetMember(Key).FirstOrDefault() : null;
            }
        }

        public ConfigFieldInfo(string name, Type configType, Type elemType, Mode mode, bool loadAll, string key, bool boot)
        {
            ConfigType = configType;
            Name = name;
            ElemType = elemType;
            FieldMode = mode;
            LoadAll = loadAll;
            Key = key;
            Boot = boot;
        }
    }
}
