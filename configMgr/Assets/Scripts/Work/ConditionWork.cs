using System;

namespace Kernel.Runtime
{
	public class ConditionWork : CommonWork
	{
		private readonly Func<bool> condition;
		private bool conditionSatisfied;
		private readonly IWork then;

		public ConditionWork(Func<bool> condition, IWork then)
		{
			this.condition = condition;
			this.then = then;
		}

		public override float GetTotal()
		{
			return then != null ? then.GetTotal() : 1;
		}

		public override float GetCurrent()
		{
			return then != null ? then.GetCurrent() : 0;
		}

		public override string GetLabel()
		{
			return then != null ? then.GetLabel() : null;
		}

		protected override WorkState CanChangeState(WorkState currentState)
		{
			if (currentState == WorkState.PLAYING && conditionSatisfied && (then == null || then.IsFinished()))
			{
				return WorkState.FINISHED;
			}
			return base.CanChangeState(currentState);
		}

		protected override void DoTick(float deltaTime)
		{
			if (!conditionSatisfied)
			{
				conditionSatisfied = condition();
				if (conditionSatisfied && then != null) then.Start();
			}
			if (conditionSatisfied && then != null)
			{
				then.Tick(deltaTime);
			}
		}
	}
}