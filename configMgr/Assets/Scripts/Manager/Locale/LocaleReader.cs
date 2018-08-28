using System.Collections.Generic;
using System.Linq;
using Kernel.Game;
using Kernel.Lang.Extension;

namespace Kernel
{
	public class LocaleReader
	{
		public static void DisableLocale()
		{
			Locale.disabled = true;
		}

		public static void Clear()
		{
			Locale.hashedLocales.Clear();
			Locale.unHashedLocales.Clear();
		}

		public static void AddLocaleFromFile(string file)
		{
			if (!PlatformManager.Instance.FileExists(file))
				return;
			var needMarkText = false;
			var externalPath = PriorityPathManager.Instance.ExternalPathFirst(file);
			var lines = PlatformManager.Instance.ReadAllLines(externalPath);
			for (int i = 0; i + 1 < lines.Length; i += 2)
			{
				var key = lines[i].Trim();
				var keyHash = key.GetHashCode();
				var content = lines[i + 1];
				var value = DecodeText(content, needMarkText);
				Locale.hashedLocales[keyHash] = value;
			}
		}

		public static void AddLocaleFromFolder(string folder)
		{
			var needMarkText = false;
			var valueIntern = new Dictionary<string, string>();
			var locales = new Dictionary<string, string>();
			var hashCount = new Dictionary<int, int>();
			var externalFiles = new HashSet<string>();

			if (PlatformManager.Instance.DirectoryExists(folder))
			{
				foreach (var path in PlatformManager.Instance.GetFiles(folder, "*", false))
				{
					var externalPath = PriorityPathManager.Instance.ExternalPathFirst(path);
					if (PlatformManager.Instance.IsEditor || PlatformManager.Instance.Platform == Platform.WINDOWS)
						externalPath = externalPath.UnixLike();
					if (PathManager.Instance.IsExternalPath(externalPath))
						externalFiles.Add(externalPath);
					var lines = PlatformManager.Instance.ReadAllLines(externalPath);
					for (int i = 0; i + 1 < lines.Length; i += 2)
					{
						var key = lines[i].Trim();
						var keyHash = key.GetHashCode();
						var content = lines[i + 1];
#if UNITY_EDITOR
						if (locales.ContainsKey(key))
						{
							Logger.Warn("请策划处理：本地化(locale)代号重复'{0}'，对应的文字为：'{1}'，来自文件{2}。{3}",
								key, content, path, GuessReason(key));
						}
#endif
						if (!hashCount.ContainsKey(keyHash))
							hashCount[keyHash] = 1;
						else
							++hashCount[keyHash];

						string value = DecodeText(content, needMarkText);
						string interned;
						if (valueIntern.TryGetValue(value, out interned))
						{
							locales[key] = interned;
						}
						else
						{
							valueIntern[value] = value;
							locales[key] = value;
						}
					}
				}
			}

			foreach (var locale in locales)
			{
				var keyHash = locale.Key.GetHashCode();
				if (hashCount[keyHash] > 1)
					Locale.unHashedLocales[locale.Key] = locale.Value;
				else
					Locale.hashedLocales[keyHash] = locale.Value;
			}

			if (PathManager.Instance.IsInternalPath(folder))
			{
				foreach (var f in PlatformManager.Instance.GetFiles(PathManager.Instance.ChangeToExternalPath(folder), "*", false))
				{
					var file = f;
					if (PlatformManager.Instance.IsEditor || PlatformManager.Instance.Platform == Platform.WINDOWS)
						file = file.Replace('/', '\\');
					if (externalFiles.Contains(file))
						continue;
					AddLocaleFromFile(file);
				}
			}
		}

		////////////////////////////////////////////////////////////////
		// impl
		private static string DecodeText(string str, bool debugMarkText = false)
		{
			if (str.IndexOf("\\") != -1)
			{
				str = str.Replace("\\n", "\n");
				str = str.Replace("\\\\", "\\");
			}
			return debugMarkText ? string.Format("##{0}##", str) : str;
		}

		private static object GuessReason(string key)
		{
			if (!IsAscii(key))
			{
				return "代号应当是英文，此处代号包含中(日）文，是否填错位了？";
			}
			return "";
		}

		private static bool IsAscii(string value)
		{
			return value.All(o => (int)o < 128);
		}
	}
}