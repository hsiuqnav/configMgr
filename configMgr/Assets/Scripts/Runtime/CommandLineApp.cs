
using Alche.Runtime;
using Kernel.Game;

namespace Kernel.Runtime
{
	public abstract class CommandLineApp : IApp
	{
		static CommandLineApp()
		{
			//XmlUtil.InitDerivedClassList(typeof(CommandLineApp));
		}

		public float DeltaTime
		{
			get
			{
				return 1 / 30f;
			}
		}

		public float UnscaledDeltaTime
		{
			get
			{
				return 1 / 30f;
			}
		}

		public bool Running
		{
			get;
			private set;
		}

		public CommandLineApp()
		{
			ModuleManager.Instance.RegisterModule(typeof(PlatformModule), () => new CommandLinePlatformModule());
			ModuleManager.Instance.RegisterModule(typeof(BasePathModule), () => new CommandLinePathModule("../../content"));
		}

		public virtual void Awake()
		{
			Running = true;
			Game.Instance.Init(CreateEnterState(Game.Instance), this);
		}

		public void Update()
		{
			Game.Instance.Tick(DeltaTime);
			if (Game.Instance.GetFSMState() is GameQuitState)
				Running = false;
		}

		public void LateUpdate()
		{
			Game.Instance.LateTick();
		}

		public void OnApplicationPause(bool pause)
		{

		}

		public void OnApplicationFocus(bool focus)
		{

		}

		public void OnApplicationQuit()
		{
		}

		protected virtual GameFSMState CreateEnterState(Game game)
		{
			return new GameInitState(game);
		}
	}
}