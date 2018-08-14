using System;
using System.Collections.Generic;
using Kernel.Game;
using Kernel.Runtime;

namespace Kernel
{
	public class PriorityPathManager : Manager<PriorityPathManager>
	{
		private readonly Dictionary<string, string> externalPathMap = new Dictionary<string, string>();
		private static Object thread_locker = new Object();

		public string ExternalPathFirst(string path)
		{
			lock (thread_locker)
			{
				var externalPath = (string)null;
				if (externalPathMap.TryGetValue(path, out externalPath))
				{
					return externalPath;
				}
				if (PathManager.Instance.IsInternalPath(path))
				{
					externalPath = PathManager.Instance.ChangeToExternalPath(path);
					if (PlatformManager.Instance.FileExists(externalPath))
					{
						externalPathMap.Add(path, externalPath);
						return externalPath;
					}
				}
				return path;
			}
		}

		public void AddPriorityPath(string internalPath, string externalPath)
		{
			if (!externalPathMap.ContainsKey(internalPath))
			{
				externalPathMap.Add(internalPath, externalPath);
			}
		}
	}
}
