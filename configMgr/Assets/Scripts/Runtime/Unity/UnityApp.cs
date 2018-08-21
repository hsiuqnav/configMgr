using Kernel.Config;
using Kernel.Runtime;
using UnityEngine;

namespace Alche.Runtime
{
	public class UnityApp : MonoBehaviour, IApp
	{
		private int applicationPaused;
		public float DeltaTime
		{
			get
			{
				if (applicationPaused > 0)
				{
					return 0;
				}
				return Time.deltaTime;
			}
		}

		public float UnscaledDeltaTime
		{
			get
			{
				if (applicationPaused > 0)
				{
					return 0;
				}
				return Time.unscaledDeltaTime;
			}
		}

		public void Awake()
		{
			ModuleManager.Instance.RegisterModule(typeof(ConfigReaderModule), () => new BinaryConfigReaderModule());
			Game.Instance.Init(CreateEnterState(Game.Instance), this);
		}

		public void OnApplicationFocus(bool focus)
		{
		}

		public void OnApplicationPause(bool pause)
		{
			if (!pause)
			{
				applicationPaused = 2;
			}
		}

		public void OnApplicationQuit()
		{
			Game.Instance.Quit();
		}

		public void Update()
		{
			if (applicationPaused > 0)
			{
				applicationPaused--;
			}
			Game.Instance.Tick(DeltaTime);
		}

		protected GameFSMState CreateEnterState(Game content)
		{
			return new GameInitState(content);
		}
	}
}
