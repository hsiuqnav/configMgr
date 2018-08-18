using Kernel.Game;
using Kernel.Runtime;
using System;

namespace Alche.Runtime
{
	public class CodeGenApp : CommandLineApp
	{
		private readonly string targetFolder;
		private readonly string[] targetNamespaces;

		public static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: codegen <csfolder> [namespace1;namespace2]");
				return;
			}

			CodeGenApp app = new CodeGenApp(args[0], args.Length >= 2 ? args[1] : null);
			app.Awake();

			while (app.Running)
			{
				app.Update();
				app.LateUpdate();
			}
		}

		public CodeGenApp()
		{
			System.Console.WriteLine("Register CommandLinePathModule");
			ModuleManager.Instance.RegisterModule(typeof(BasePathModule), () => new CommandLinePathModule("../resource/content"));
		}

		public CodeGenApp(string targetFolder, string targetNamespaces) : this()
		{
			this.targetFolder = targetFolder;
			this.targetNamespaces = targetNamespaces != null ? targetNamespaces.Trim('"').Split(';') : null;
		}

		protected override GameFSMState CreateEnterState(Game game)
		{
			return new GameCodeGenState(game, targetFolder, targetNamespaces);
		}
	}
}
