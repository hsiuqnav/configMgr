using System.IO;

namespace Kernel.Config
{
    public interface IConfigSerializer
    {
        object Read(IBinaryReader reader, object o);
        void Write(IBinaryWriter writer, object o);
    }
}
