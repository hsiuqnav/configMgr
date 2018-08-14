using System;

namespace Kernel.Runtime
{
	public class AtomWork : CommonWork
	{
		private readonly Action work;
		private readonly string label;
		private readonly float weight;

		public AtomWork(Action work, string label = null, float weight = 1)
		{
			this.work = work;
			this.weight = weight;
			this.label = label;
		}

		public override float GetTotal()
		{
			return weight;
		}

		public override float GetCurrent()
		{
			return IsFinished() ? weight : 0;
		}

		public override string GetLabel()
		{
			return label;
		}

		protected override void DoStart()
		{
			if (work != null) work();
			ChangeState(WorkState.FINISHED);
		}
	}
}