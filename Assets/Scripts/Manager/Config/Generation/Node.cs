using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kernel.Config
{
    public abstract class Node
    {
        public Node Next;
        public Node Prev;
        public Node FirstChild;
        public Node LastChild;
        public Node Parent;

        public readonly Type Type;

        public string TypeName
        {
            get
            {
                if (typeName == null)
                {
                    typeName = TypeUtil.GetCSharpFullTypeName(Type);
                }
                return typeName;
            }
        }

        public IEnumerable<Node> Children
        {
            get
            {
                Node p = FirstChild;
                while (p != null)
                {
                    yield return p;
                    p = p.Next;
                }
            }
        }

        private string typeName;

        protected Node(Type type)
        {
            Type = type;
        }

        public void Accept()
        {

        }

        public virtual void Generate(HashSet<RootType> referTypes)
        {

        }

        public void AddChild(Node node, HashSet<RootType> referTypes)
        {
            if (node == null) return;

            node.Generate(referTypes);

            if (LastChild != null)
            {
                LastChild.Next = node;
                node.Prev = LastChild;
                node.Next = null;
                node.Parent = this;
                LastChild = node;
            }
            else if (FirstChild == null)
            {
                FirstChild = LastChild = node;
                node.Parent = this;
                node.Prev = node.Next = null;
            }
        }

        protected List<FieldInfo> GetSerializeFields(Type type)
        {
            var attributes = new[]
            {
                typeof(NonSerializedAttribute)
            };
            var fields = TypeUtil.GetPublicInstanceFieldsExcept(type, attributes);
            fields.RemoveAll(o => TypeUtil.IsDelegation(o.FieldType));
            fields.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return fields;
        }
    }
}
