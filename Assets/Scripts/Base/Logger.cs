using UnityEngine;

namespace Kernel
{
	public static class Logger
	{
		public static void Warn(string content, params object[] args)
		{
#if UNITY_EDITOR
			Debug.LogWarningFormat(content, args);
#endif
		}

		public static void Fatal(string content, params object[] args)
		{
#if UNITY_EDITOR
			Debug.LogErrorFormat(content, args);
#endif
		}

		public static void Trace(string content, params object[] args)
		{
#if UNITY_EDITOR
			Debug.LogFormat(content, args);
#endif
		}

		public static void Error(string content, params object[] args)
		{
#if UNITY_EDITOR
			Debug.LogErrorFormat(content, args);
#endif
		}

		public static void Info(string content, params object[] args)
		{
#if UNITY_EDITOR
			Debug.LogFormat(content, args);
#endif
		}
	}
}
