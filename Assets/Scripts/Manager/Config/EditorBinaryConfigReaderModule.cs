using Kernel.Runtime;

namespace Kernel.Config
{
	public class EditorBinaryConfigReaderModule : BinaryConfigReaderModule
	{
		public override IWork LoadConfigAutomatically()
		{
			return new AtomWork(ReadAll);
		}
	}
}
