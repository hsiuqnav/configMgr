namespace Kernel.Game
{
	public class EditorPathModule : BasePathModule
	{
		private const string EXTERNAL_LOG_FILE = "{ExternalFolder}/../log/log_{0}.txt";
		private const string EXTERNAL_SAVE_FOLDER = "{ExternalFolder}/../save";

		public override string BundleFolder
		{
			get
			{
				return "Windows";
			}
		}

		public override string ExternalFolder
		{
			get
			{
				return GetFullPath("../../resource/content");
			}
		}

		public override string ExternalSaveFolder
		{
			get
			{
				return Parse(EXTERNAL_SAVE_FOLDER);
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
			return Parse(EXTERNAL_LOG_FILE, index);
		}
	}
}