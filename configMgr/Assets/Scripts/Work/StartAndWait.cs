using System;

namespace Kernel.Runtime
{
	public class StartAndWait : CommonWork
	{
		private readonly IWork[] works;
		private readonly float total;
		private bool lastStepSuccessed;

		public StartAndWait(params IWork[] works)
		{
			this.works = works;
			foreach (var work in works)
			{
				if (work != null) total += work.GetTotal();
			}
		}

		public SequenceWork CastToSequenceWork()
		{
			return new SequenceWork(null, works);
		}

		public override float GetTotal()
		{
			return total;
		}

		public override float GetCurrent()
		{
			float current = 0;
			for (int i = 0; i < works.Length; ++i)
			{
				if (works[i] == null) continue;
				if (works[i].IsFinished()) current += works[i].GetTotal();
				else current += works[i].GetCurrent();
			}
			return current;
		}

		public override string GetLabel()
		{
			for (int i = 0; i < works.Length; ++i)
			{
				if (works[i] == null) continue;
				if (!works[i].IsFinished() && works[i].IsStarted() && works[i].GetLabel() != null)
					return works[i].GetLabel();
			}
			return null;
		}

		protected override void DoStart()
		{
			StartPendingWorks();
			lastStepSuccessed = true;
		}

		protected override WorkState CanChangeState(WorkState currentState)
		{
			if (currentState == WorkState.PLAYING)
			{
				bool completed = true;
				for (int i = 0; i < works.Length; ++i)
				{
					if (works[i] == null) continue;
					if (!works[i].IsFinished())
					{
						completed = false;
						break;
					}
				}
				if (completed)
				{
					return WorkState.FINISHED;
				}
			}
			return base.CanChangeState(currentState);
		}

		protected override void DoTick(float deltaTime)
		{
			if (!lastStepSuccessed)
				return;

			// 不使用try catch，因为try catch之后Excetption的栈会被截断到这个Tick处。
			// 里面也不能写 return
			lastStepSuccessed = false;
			for (int i = 0; i < works.Length; ++i)
			{
				if (works[i] != null && !works[i].IsFinished())
					works[i].Tick(deltaTime);
			}

			StartPendingWorks();
			lastStepSuccessed = true;
		}

		private void StartPendingWorks()
		{
			DateTime begin = DateTime.Now;
			int startedCount = 0;
			for (int i = 0; i < works.Length; ++i)
			{
				if (works[i] == null) continue;
				if ((DateTime.Now - begin).TotalSeconds > 0.1)
					return;
				if (startedCount > 0 && !works[i].IsStarted() && works[i].GetTotal() > 0.1)
					return;

				if (!works[i].IsStarted())
				{
					works[i].Start();
					++startedCount;
				}
			}
		}
	}
}