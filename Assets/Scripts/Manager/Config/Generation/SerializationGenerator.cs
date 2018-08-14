using System;
using System.Collections.Generic;
using System.Linq;

namespace Kernel.Config
{
    public class SerializationGenerator
    {
        public IEnumerable<RootNode> RegisteredTypes
        {
            get
            {
                return registeredTypes.Values.OrderBy(v => v.Type.FullName);
            }
        }

        private readonly Dictionary<RootType, RootNode> registeredTypes = new Dictionary<RootType, RootNode>();

        public void RegisterRoot(Type type)
        {
            var root = Generate(new RootType(type));
            if (root != null && !root.IsEnum)
            {
                root.IsEntry = true;
            }
        }

        public void Register(Type type)
        {
            Generate(new RootType(type));
        }

        private RootNode Generate(RootType type)
        {
            if (type.Type == null) return null;

            if (registeredTypes.ContainsKey(type))
                return registeredTypes[type];

            RootType[] referencedTypes;

            RootNode root = registeredTypes[type] = Generate(type, out referencedTypes);

            if (referencedTypes != null)
            {
                for (int i = 0; i < referencedTypes.Length; i++)
                {
                    Generate(referencedTypes[i]);
                }
            }

            return root;
        }

        private RootNode Generate(RootType type, out RootType[] referencedTypes)
        {
            registeredTypes.Add(type, null);

            HashSet<RootType> referTypes = new HashSet<RootType>();
            RootNode root = new RootNode(type);
            root.Generate(referTypes);
            referencedTypes = referTypes.ToArray();
            return root;
        }
    }
}
