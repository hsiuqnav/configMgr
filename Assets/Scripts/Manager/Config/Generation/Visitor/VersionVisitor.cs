using System.Collections.Generic;

namespace Kernel.Config
{
    public class VersionVisitor
    {
        public string GenerateMd5(SerializationGenerator generator)
        {
            List<byte> codeMd5 = new List<byte>();

            foreach (var node in generator.RegisteredTypes)
            {
                if (node.CodeMd5 != null) codeMd5.AddRange(node.CodeMd5);
            }

            return MD5Util.Md5ToString(MD5Util.MD5(codeMd5.ToArray()));
        }
    }
}
