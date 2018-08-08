using System;
using System.Collections.Generic;

namespace Kernel.Lang.Extension
{
	public static class ExtendIDictionary
	{
		public static TValue GetEx<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
		{
			return dict.GetEx(key, default(TValue));
		}

		public static TValue GetEx<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
		{
			TValue v;
			if (null != dict && dict.TryGetValue(key, out v))
			{
				return v;
			}

			return defaultValue;
		}

		public static TValue SetDefaultEx<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
		{
			return dict.SetDefaultEx(key, default(TValue));
		}

		public static TValue SetDefaultEx<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
		{
			if (null != dict)
			{
				TValue value;
				if (!dict.TryGetValue(key, out value))
				{
					value = defaultValue;
					dict.Add(key, value);
				}

				return value;
			}

			return default(TValue);
		}

		public static TValue AddEx<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue v)
		{
			if (null != dict)
			{
				dict.Add(key, v);
			}

			return v;
		}

		public static void AddRangeEx<TKey, TValue>(this IDictionary<TKey, TValue> dictTo, IDictionary<TKey, TValue> dictFrom)
			where TKey : IComparable
		{
			if (null != dictTo && null != dictFrom)
			{
				foreach (var pair in dictFrom)
				{
					dictTo[pair.Key] = pair.Value;
				}
			}
		}
	}
}