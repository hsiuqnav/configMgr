using System;
using System.Collections.Generic;
using Kernel.Lang.Collection;
using Kernel.Util;

namespace Kernel
{
	public static class Locale
	{
		public static bool disabled;

		// 此处已经做了内存优化： 
		// 1. 使用 hashedLocales 为主，仅在碰撞情况下使用unHashedLocales
		// 2. 对value进行 intern
		// 在实际项目44000个数据测试中，内存占用由 7M -> 4.55M；对于第一项测试，实际碰撞为0。
		public static readonly Dictionary<int, string> hashedLocales = new Dictionary<int, string>();
		public static readonly Dictionary<string, string> unHashedLocales = new Dictionary<string, string>(StringEqualityComparer.Instance);
		private static Dictionary<string, string> specialLocaleCache = new Dictionary<string, string>();

		public static bool DebugLocale { get; set; }

		public static bool ContainsKey(string key)
		{
			return hashedLocales.ContainsKey(key.GetHashCode()) || unHashedLocales.ContainsKey(key);
		}

		public static string L(string path)
		{
			if (disabled || path == null)
				return null;

			path = path.Trim();
			string content = GetContent(path);

			if (content == null)
			{
				return string.Format("!!{0}!!", path);
			}

			if (DebugLocale)
			{
				return "★";
			}

			return content;
		}

		public static string L(string path, params object[] args)
		{
			if (disabled || path == null)
				return null;

			path = path.Trim();
			string content = GetContent(path);

			if (content == null)
			{
				return string.Format("!!{0}!!", path);
			}

			if (DebugLocale)
			{
				return "★";
			}

			return String.Format(content, args);
		}

		public static string L(KernelResult result)
		{
#if UNITY_EDITOR
			if (!Locale.Has("kernel.result." + (int)result))
				return result.ToString();
#endif
			return Locale.L("kernel.result." + (int)result);
		}

		public static bool Has(string path)
		{
			return !disabled && path != null && ContainsKey(path.Trim());
		}

		public static TempList<string> LArray(string key)
		{
			if (string.IsNullOrEmpty(key))
				return null;

			var retStrs = TempList<string>.Alloc();
			int i = 1;
			string str;
			do
			{
				string elemKey = string.Format("{0}.{1}", key, i);
				str = Has(elemKey) ? L(elemKey) : null;
				if (str != null)
					retStrs.Add(str);
				++i;
			}
			while (str != null);

			return retStrs;
		}

		////////////////////////////////////////////////////////////////
		// impl
		private static string GetContent(string path)
		{
			string retLocaleStr;
			if (path != null && (hashedLocales.TryGetValue(path.GetHashCode(), out retLocaleStr) || unHashedLocales.TryGetValue(path, out retLocaleStr)))
			{
				if (retLocaleStr != null && retLocaleStr.Contains("{"))
				{
					if (specialLocaleCache.ContainsKey(retLocaleStr))
					{
						return specialLocaleCache[retLocaleStr];
					}
					else
					{
						bool changed;
						string str = StringUtil.ReplaceSpecialToken(retLocaleStr, out changed);

						if (changed)
						{
							specialLocaleCache.Add(retLocaleStr, str);
						}
						return str;
					}
				}
				return retLocaleStr;
			}
#if UNITY_EDITOR
			Logger.Warn("locale path [{0}] not found", path);
#endif
			return null;
		}
	}
}