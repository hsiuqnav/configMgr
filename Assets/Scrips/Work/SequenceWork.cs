using System;
using System.Collections.Generic;
using Kernel.Util;

namespace Kernel.Runtime
{
	public class SequenceWork : CommonWork
	{
		private readonly IList<IWork> works;
		private readonly float total;
		private readonly string label;
		private float completed;
		private int current;
		private bool lastStepSuccessed;

		public SequenceWork(string label, IList<IWork> works)
		{
			this.label = label;
			this.works = works;

			foreach (var work in works)
				total += work.GetTotal();
		}

		public SequenceWork(string label, params IWork[] works) : this(label, (IList<IWork>)works)
		{

		}

		public override float GetTotal()
		{
			return total;
		}

		public override float GetCurrent()
		{
			if (current < works.Count)
				return completed + (works[current].IsStarted() ? works[current].GetCurrent() : 0);
			return total;
		}

		public override string GetLabel()
		{
			if (current < works.Count)
				return works[current].GetLabel() ?? label;
			return label;
		}

		protected override WorkState CanChangeState(WorkState currentState)
		{
			if (currentState == WorkState.PLAYING && current == works.Count)
			{
				return WorkState.FINISHED;
			}
			return base.CanChangeState(currentState);
		}

		protected override void DoStart()
		{
			StartCurrentWorks();
			lastStepSuccessed = true;
		}

		protected override void DoTick(float deltaTime)
		{
			if (!lastStepSuccessed)
				return;

			// 不使用try catch，因为try catch之后Excetption的栈会被截断到这个Tick处。
			// 里面也不能写 return
			lastStepSuccessed = false;
			if (current < works.Count)
			{
				if (!works[current].IsStarted())
				{
					StartCurrentWorks();
				}
				else
				{
					if (!works[current].IsFinished())
						works[current].Tick(deltaTime);

					if (works[current].IsFinished())
					{
						completed += works[current].GetTotal();
						++current;
						StartCurrentWorks();
					}
				}
			}
			lastStepSuccessed = true;
		}

		private void StartCurrentWorks()
		{
			DateTime begin = DateTime.Now;
			int startedCount = 0;
			while (current < works.Count)
			{
				if ((DateTime.Now - begin).TotalSeconds > 0.1)
					return;
				if (startedCount > 0 && works[current].GetTotal() > 0.1)
					return;

				Global.Assert(!works[current].IsStarted(), "work is already started");
				works[current].Start();
				++startedCount;
				if (works[current].IsFinished())
				{
					completed += works[current].GetTotal();
					++current;
				}
				else
				{
					break;
				}
			}
		}
	}
}
