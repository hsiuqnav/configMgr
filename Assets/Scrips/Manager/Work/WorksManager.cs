using System.Collections.Generic;

namespace Kernel.Runtime
{
    public class WorksManager : Manager<WorksManager>
    {
        private int runningCount;
        private int lastTickFrame;
        private readonly List<IWork> runningWorks = new List<IWork>();
        private readonly HashSet<IWork> allWorks = new HashSet<IWork>();

        public IEnumerable<object> Iterator(IEnumerable<object> enumerable)
        {
            foreach (var current in enumerable)
            {
                if (current is IEnumerable<object>)
                {
                    foreach (var e in Iterator(current as IEnumerable<object>))
                    {
                        yield return e;
                    }
                }
                else
                {
                    yield return current;
                }
            }
        }

        public void AddStartRightAwayWork(IWork work)
        {
            if (work != null && !allWorks.Contains(work))
            {
                AddWork(work);
                work.Start();
                TickOnEditorMode();
            }
        }

        public void AddStartRightAwayWorks(IList<IWork> works)
        {
            if (works == null) return;
            for (int i = 0; i < works.Count; i++)
            {
                if (works[i] != null) AddStartRightAwayWork(works[i]);
            }
        }

        public void AddWork(IWork work)
        {
            if (work != null && !allWorks.Contains(work))
            {
                allWorks.Add(work);
                if (runningCount < runningWorks.Count)
                {
                    runningWorks[runningCount++] = work;
                }
                else
                {
                    runningWorks.Add(work);
                    runningCount++;
                }
            }
        }

        public void AddWorks(IList<IWork> works)
        {
            if (works == null) return;
            for (int i = 0; i < works.Count; i++)
            {
                if (works[i] != null) AddWork(works[i]);
            }
        }

        public void StartWorks(IList<IWork> works)
        {
            if (works == null) return;
            for (int i = 0; i < works.Count; i++)
            {
                if (works[i] != null) works[i].Start();
            }
        }

        protected override void OnBoot()
        {
            allWorks.Clear();
        }

        protected override void OnTick(float deltaTime)
        {
            int k = 0;
            for (int i = 0; i < runningCount; i++)
            {
                var work = runningWorks[i];
                if (work.IsStarted())
                    work.Tick(deltaTime);
                if (!work.IsFinished())
                {
                    runningWorks[k] = work;
                    k++;
                }
            }
            for (int i = k; i < runningCount; i++)
            {
                runningWorks[i] = null;
            }
            runningCount = k;
        }

        private void TickOnEditorMode()
        {
#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
            {
                while (runningCount > 0)
                {
                    OnTick(1 / 30f);
                }
            }
#endif
        }
    }
}
