using System;
using System.Collections.Generic;
using Kernel.Lang.Collection;

namespace Kernel.Runtime
{
	public class ManagerMan : Singleton<ManagerMan>
	{
		public Action OnBootLoadFinished;

		private readonly List<IManager> managers = new List<IManager>();
		private readonly List<IWork> initWorks = new List<IWork>();
		private bool roleDataInited;
		public void UnregisterManager(IManager manager)
		{
			if (manager != null)
			{
				managers.Remove(manager);
			}
		}

		public void RegisterManager(IManager manager)
		{
			if (manager == null) return;
			managers.Add(manager);
		}

		public void ResetAllManagers()
		{
			for (int index = 0; index < managers.Count; index++)
			{
				managers[index].Reset();
			}
		}

		public void InitAllManagers()
		{
			for (int index = 0; index < managers.Count; index++)
			{
				managers[index].Init();
			}
		}

		public void BootAllManagers(bool reboot = false)
		{
			roleDataInited = false;
			for (int index = 0; index < managers.Count; index++)
			{
				managers[index].Boot(reboot);
			}
		}

		public IWork BootLoadAllManagers()
		{
			var loaders = TempList<IWork>.Alloc();
			for (int index = 0; index < managers.Count; index++)
			{
				var loader = managers[index].BootLoad();
				if (loader != null) loaders.Add(loader);
			}
			if (loaders.Count > 0)
			{
				return new ParallelWork("", loaders);
			}
			return new CommonWork();
		}

		public void BootLoadFinished()
		{
			for (int index = 0; index < managers.Count; index++)
			{
				managers[index].BootLoadFinished();
			}
			if (OnBootLoadFinished != null) OnBootLoadFinished();
		}

		public void ShutdownAllManagers()
		{
			for (int index = 0; index < managers.Count; index++)
			{
				managers[index].Shutdown();
			}
		}

		public void QuitAllManagers()
		{
			for (int index = 0; index < managers.Count; index++)
			{
				managers[index].Quit();
			}
		}

		public void Tick(float deltaTime)
		{
			for (int index = 0; index < managers.Count; index++)
			{
				managers[index].Tick(deltaTime);
			}
			if (initWorks.Count > 0)
			{
				bool finished = true;
				foreach (var work in initWorks)
				{
					if (!work.IsFinished())
					{
						finished = false;
						break;
					}
				}
			}
		}

		public void LateTick()
		{
			for (int index = 0; index < managers.Count; index++)
			{
				managers[index].LateTick();
			}
		}
	}
}