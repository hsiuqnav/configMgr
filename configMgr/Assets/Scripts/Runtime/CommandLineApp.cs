
namespace Kernel.Runtime
{
	public abstract class CommandLineApp : IApp
	{
		static CommandLineApp()
		{
			XmlUtil.InitDerivedClassList(typeof(CommandLineApp));
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

		public virtual void Awake()
		{
			Running = true;
			//Game.Instance.Init(CreateEnterState(Game.Instance), this);
		}

		public void Update()
		{
			//Game.Instance.Tick(DeltaTime);
			//if (Game.Instance.GetFSMState() is GameQuitState)
			//	Running = false;
		}

		public void LateUpdate()
		{
			//Game.Instance.LateTick();
		}

		public void OnApplicationPause(bool pause)
		{

		}

		public void OnApplicationFocus(bool focus)
		{

		}

		public void OnApplicationQuit()
		{
			//Logger.RemoveAppender<CommandLineLogSpi>();
		}

		//protected abstract GameFSMState CreateEnterState(Game game);
	}
}