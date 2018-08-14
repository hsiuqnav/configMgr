using UnityEngine;

namespace Kernel
{
    public static class Logger
    {
        public static void Warn(string content, params object[] args)
        {
            Debug.LogWarningFormat(content, args);
        }

        public static void Fatal(string content, params object[] args)
        {
            Debug.LogErrorFormat(content, args);
        }

        public static void Trace(string content, params object[] args)
        {
            Debug.LogFormat(content, args);
        }

        public static void Error(string content, params object[] args)
        {
            Debug.LogErrorFormat(content, args);
        }

		public static void Info(string content, params object[] args)
		{
			Debug.LogFormat(content, args);
		}
    }
}
