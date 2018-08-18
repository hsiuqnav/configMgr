using System;
using System.Collections.Generic;
using Event = Kernel.FSM.Event;

namespace Kernel.Runtime
{
	public class Game : Singleton<Game>
	{
		public Action OnGameQuit; // 会在Quit的时候第一时间调用
		private GameFSM fsm;
		private Queue<Event> eventQueue = new Queue<Event>();
		private bool isQuiting;
		private IApp app;

		public static bool IsQuiting
		{
			get
			{
				return Instance.isQuiting;
			}
		}

		public float UnscaledDeltaTime
		{
			get
			{
				return app.UnscaledDeltaTime;
			}
		}

		public GameFSMState GetFSMState()
		{
			return fsm != null ? fsm.CurrentState as GameFSMState : null;
		}

		public string GetFSMStateName()
		{
			var state = GetFSMState();
			return state != null ? state.GetType().Name : null;
		}

		public void Init(GameFSMState enterState, IApp app)
		{
			fsm = new GameFSM(this, enterState);
			fsm.Reset();
			this.app = app;
		}

		public void InputEvent(int eventType)
		{
			fsm.Input(new Event(eventType));
		}

		public void InputGameEvent(string eventType, object parameter = null)
		{
			fsm.Input(new GameEvent(eventType, parameter));
		}

		public void EnqueueGameEvent(string eventType, object parameter = null)
		{
			eventQueue.Enqueue(new GameEvent(eventType, parameter));
		}

#if !DISABLE_UNITY
		public void StartLoadingAndInputGameEvent(string eventType, object parameter = null)
		{
			WorksManager.Instance.RunCoroutine(LoadingAndInput(eventType, parameter));
		}

		private IEnumerator<object> LoadingAndInput(string eventType, object parameter)
		{
			yield return null;
			InputGameEvent(eventType, parameter);
		}
#endif

		public void InputGameEvent(GameEvent evt)
		{
			fsm.Input(evt);
		}

		public void Quit()
		{
			isQuiting = true;
			if (OnGameQuit != null)
			{
				OnGameQuit();
			}
			ManagerMan.Instance.ShutdownAllManagers();
			ManagerMan.Instance.QuitAllManagers();
			PlatformManager.Instance.QuitGame();
		}


		public void UpdateLoadingProgress(float progress)
		{
		}

		public void Tick(float deltaTime)
		{
			while (eventQueue.Count > 0)
			{
				var e = eventQueue.Dequeue();
				fsm.Input(e);
			}
			fsm.Tick(deltaTime);
			ManagerMan.Instance.Tick(deltaTime);
		}

		public void LateTick()
		{
			ManagerMan.Instance.LateTick();
		}
	}
}