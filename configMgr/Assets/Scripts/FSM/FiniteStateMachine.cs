using Kernel.Engine;

namespace Kernel.FSM
{
	public interface IFiniteStateMachine
	{
		bool IsFinited
		{
			get;
		}

		void Stop();
	}

	public class FiniteStateMachine<T, TTick> : IFiniteStateMachine
	{
		protected delegate State StateConstructor();
		private StateConstructor enterStateConstructor;
		private readonly State enterState;
		private Event keptEvent;

		protected FiniteStateMachine(T content)
		{
			if (content == null)
			{
				Logger.Fatal("FiniteStateMachine with content == null");
			}
			Content = content;
		}

		protected FiniteStateMachine(T content, State enterState)
		{
			if (content == null)
			{
				Logger.Fatal("FiniteStateMachine with content == null");
			}
			Content = content;
			this.enterState = enterState;
		}

		public T Content
		{
			get;
			private set;
		}

		public State CurrentState
		{
			get;
			protected set;
		}

		protected StateConstructor EnterStateConstructor
		{
			set
			{
				enterStateConstructor = value;
			}
		}

		public virtual void Stop()
		{
			ChangeCurrentState(null, FiniteEvent.Instance);
		}

		public virtual bool IsFinited
		{
			get
			{
				return CurrentState == null;
			}
		}

		public virtual bool Input(Event e)
		{
			if (CurrentState == null)
			{
				return false;
			}
			var caller = CurrentState;
			var nextState = CurrentState.Input(e);
			if (caller != CurrentState)
			{
				return false;
			}
			if (nextState != CurrentState)
			{
				bool result = ChangeCurrentState(nextState, e);
				if (e != null && e.KeepForNextState(nextState))
				{
					keptEvent = e.NextEvent;
					if (keptEvent != null)
					{
						Input(keptEvent);
					}
				}
				else
				{
					keptEvent = null;
				}
				return result;
			}
			return false;
		}

		public void Reset()
		{
			var startState = CreateEnterState();
			var e = new ResetEvent();
			ChangeCurrentState(startState, e);
		}

		public void ForceSetCurrentState(State state)
		{
			CurrentState = state;
		}

		public bool Tick(TTick deltaTime)
		{
			if (CurrentState != null)
			{
				var caller = CurrentState;
				var nextState = caller.Tick(deltaTime);
				if (caller != CurrentState)
				{
					return false;
				}
				if (nextState != CurrentState)
				{
					if (keptEvent != null && keptEvent.KeepForNextState(nextState))
					{
						keptEvent = keptEvent.NextEvent;
					}
					else
					{
						keptEvent = null;
					}
					bool result = ChangeCurrentState(nextState, keptEvent ?? TickEvent<TTick>.GetInstance(deltaTime));
					return result;
				}
			}
			return false;
		}

		protected virtual bool ChangeCurrentState(State nextState, Event e)
		{
			if (CurrentState != null)
			{
				OnPreStateChange(CurrentState, nextState);
				CurrentState.Exit(e, nextState);
			}

			var lastState = CurrentState;
			CurrentState = nextState;

			if (nextState != null)
			{
				CurrentState.Enter(e, lastState);
				OnPostStateChange(CurrentState, lastState);
			}
			return true;
		}

		protected virtual void OnPostStateChange(State currentState, State lastState)
		{
		}

		protected virtual void OnPreStateChange(State currentState, State nextState)
		{
		}

		private State CreateEnterState()
		{
			if (enterStateConstructor != null)
			{
				var state = enterStateConstructor();
				if (state != null)
				{
					return state;
				}
			}
			else if (enterState != null)
			{
				return enterState;
			}
			return null;
		}

		public class State
		{
			public State(T content, ICounter<TTick> timer)
			{
				if (content == null)
				{
					Logger.Fatal("State with content == null");
				}
				Content = content;
				Timer = timer;
			}

			public State(T content) : this(content, (ICounter<TTick>)new Counter())
			{

			}

			public T Content
			{
				get;
				private set;
			}

			public ICounter<TTick> Timer
			{
				get;
				private set;
			}

			public virtual bool IsFinished
			{
				get
				{
					return false;
				}
			}

			public virtual void Enter(Event e, State lastState)
			{
				Reset();
				OnEnter(e, lastState);
			}

			public virtual void Exit(Event e, State nextState)
			{
				OnExit(e, nextState);
			}

			public State Input(Event e)
			{
				return DoEvent(e);
			}

			public void InsideSelfTransition()
			{
				OnInsideSelfTransition();
			}

			public bool IsNeverTimeOut()
			{
				return Timer.IsInfinity();
			}

			public bool IsTimeOut()
			{
				return Timer.IsExceed();
			}

			public void NeverTimeOut()
			{
				Timer.Infinity();
			}

			public void RedefineTimer(TTick total)
			{
				Timer.Redefine(total);
			}

			public void Reset()
			{
				OnReset();
				ResetTimer();
			}

			public void ResetTimer()
			{
				Timer.Reset();
			}

			public State Tick(TTick deltaTime)
			{
				Timer.Increase(deltaTime);
				return DoTick(deltaTime);
			}

			public void TimeOut()
			{
				Timer.Exceed();
			}

			protected virtual State DoEvent(Event e)
			{
				return this;
			}

			protected virtual State DoTick(TTick deltaTime)
			{
				return this;
			}

			protected virtual void OnEnter(Event e, State lastState)
			{
			}

			protected virtual void OnExit(Event e, State nextState)
			{
			}

			protected virtual void OnInsideSelfTransition()
			{
			}

			protected virtual void OnReset()
			{
			}
		}
	}

	public class FiniteStateMachine<T> : FiniteStateMachine<T, float>
	{
		public FiniteStateMachine(T content) : base(content)
		{

		}

		public FiniteStateMachine(T content, State enterState) : base(content, enterState)
		{

		}
	}
}