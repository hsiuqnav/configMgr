using System;
using System.Collections.Generic;

namespace Kernel.Runtime
{
    public class ParallelWork : CommonWork
    {
        protected readonly List<IWork> finishedWorks = new List<IWork>();
        protected readonly IList<IWork> works;
        protected int finishedWorkCount;

        private readonly string label;
        private float total;
        private float completed;
        private int maxWorkingCount = int.MaxValue;

        public ParallelWork(params IWork[] works) : this(null, works)
        {

        }

        public ParallelWork(string label, IList<IWork> works) : this(label, int.MaxValue, works)
        {

        }

        public ParallelWork(string label, int maxWorkingCount, IList<IWork> works)
        {
            this.label = label;
            this.works = new IWork[works.Count];
            for (var i = 0; i < works.Count; i++)
            {
                total += works[i].GetTotal();
                this.works[i] = works[i];
            }
            this.maxWorkingCount = maxWorkingCount;
        }

        public ParallelWork(string label, int maxWorkingCount)
        {
            this.label = label;
            works = new List<IWork>();
            this.maxWorkingCount = maxWorkingCount;
        }

        public ParallelWork(string label)
        {
            this.label = label;
            works = new List<IWork>();
        }

        public ParallelWork()
        {
            label = string.Empty;
            works = new List<IWork>();
        }

        public virtual void AddWork(IWork work)
        {
            if (work != null)
            {
                works.Add(work);
                total += work.GetTotal();
            }
        }

        public override float GetCurrent()
        {
            if (IsFinished())
            {
                return total;
            }
            var current = 0f;
            foreach (var work in finishedWorks)
            {
                current += work == null ? 1f : work.GetCurrent();
            }
            return current;
        }

        public override string GetLabel()
        {
            return label;
        }

        public override float GetTotal()
        {
            return total;
        }

        protected override WorkState CanChangeState(WorkState currentState)
        {
            if (currentState == WorkState.PLAYING && finishedWorkCount == works.Count)
            {
                return WorkState.FINISHED;
            }
            return base.CanChangeState(currentState);
        }

        protected override void DoFinish()
        {
            finishedWorks.Clear();
        }

        protected override void DoStart()
        {
            maxWorkingCount = Math.Min(maxWorkingCount, works.Count);
            for (var i = 0; i < maxWorkingCount; ++i)
            {
                if (works[i] != null)
                {
                    works[i].Start();
                }
            }
        }

        protected override void DoTick(float deltaTime)
        {
            for (var i = 0; i < works.Count; ++i)
            {
                if (works[i] == null)
                {
                    continue;
                }
                if (!works[i].IsFinished())
                {
                    works[i].Tick(deltaTime);
                }
                if (works[i].IsFinished())
                {
                    completed += works[i].GetTotal();
                    ++finishedWorkCount;
                    finishedWorks.Add(works[i]);
                    works[i] = null;
                    continue;
                }
                if (i > maxWorkingCount - 1 && i < maxWorkingCount + completed)
                {
                    if (!works[i].IsStarted())
                    {
                        works[i].Start();
                    }
                }
            }
        }
    }
}