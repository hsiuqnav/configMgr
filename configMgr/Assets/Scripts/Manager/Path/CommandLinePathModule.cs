namespace Kernel.Game
{
	public class CommandLinePathModule : BasePathModule
	{
		private readonly string externalFolder;

		public CommandLinePathModule(string externalFolder)
		{
			this.externalFolder = externalFolder;
		}

		public override string BundleFolder
		{
			get
			{
				return null;
			}
		}

		public override string ExternalFolder
		{
			get
			{
				return GetFullPath(externalFolder);
			}
		}

		public override string InitDataFolder
		{
			get
			{
				return ExternalFolder;
			}
		}

		public override string GetExternalLogFile(int index)
		{
			return null;
		}
	}
}