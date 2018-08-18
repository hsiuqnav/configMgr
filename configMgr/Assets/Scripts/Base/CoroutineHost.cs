using System.Collections.Generic;
using UnityEngine;

namespace Kernel.Util
{
	[ExecuteInEditMode]
	public class CoroutineHost : MonoBehaviour
	{
		private static CoroutineHost instance;

		public static void SetHost(CoroutineHost host)
		{
			instance = host;
		}

		public static Coroutine Run(IEnumerator<object> coroutine)
		{
			return instance.StartCoroutine(coroutine);
		}

		public static void Stop(Coroutine coroutine)
		{
			instance.StopCoroutine(coroutine);
		}
	}
}