using System;
using System.Collections.Generic;

namespace Kernel.Runtime
{
    public class ModuleManager : Manager<ModuleManager>
    {
        private readonly Dictionary<string, Module> modules = new Dictionary<string, Module>();
        private readonly Dictionary<Type, Func<Module>> creators = new Dictionary<Type, Func<Module>>();

        public void AddModule(string moduleType, Module module)
        {
            if (modules.ContainsKey(moduleType))
            {
                var oldModule = modules[moduleType];
                modules.Remove(moduleType);
                oldModule.Unload();
            }

            modules.Add(moduleType, module);
            module.Init();
        }

        public void AddModule(Type moduleType, Module module)
        {
            AddModule(moduleType.FullName, module);
        }

        public Module AddDefaultModule(Type type)
        {
            Func<Module> creator = null;
            if (creators.TryGetValue(type, out creator))
            {
                Module m = creator();
                if (m != null)
                {
                    AddModule(type, m);
                    return m;
                }
            }
            return null;
        }

        public void RegisterModule(Type type, Func<Module> creator)
        {
            if (creators.ContainsKey(type)) creators.Remove(type);
            creators.Add(type, creator);
        }

        protected override void OnTick(float deltaTime)
        {
            foreach (KeyValuePair<string, Module> pair in modules)
            {
                if (pair.Value != null)
                {
                    pair.Value.Tick(deltaTime);
                }
            }
        }
    }
}
