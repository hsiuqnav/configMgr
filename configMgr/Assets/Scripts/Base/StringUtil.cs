using System;
using System.Collections;
using System.Text.RegularExpressions;
using Kernel.Lang;
using Kernel.Util;

namespace Kernel
{
	public class StringUtil
	{
		public static readonly char[] SPACE_CHARS =
		{
			' ', '\t', '\n', '\r'
		};

		private static readonly Regex regex = new Regex(@"\{.+\}");
		private static readonly MatchEvaluator matchEvaluator = ReplaceSpecialToken;


		private static bool SpecialTokenApplied;

		public static string BytesFormatString(long sum)
		{
			const long OneTB = 1024L * 1024L * 1024L * 1024L;
			const long OneGB = 1024 * 1024 * 1024;
			const long OneMB = 1024 * 1024;
			const long OneKB = 1024;
			if(sum >= OneTB)
			{
				return string.Format("{0:N2}TB", (double)sum / OneTB);
			}
			if(sum >= OneGB)
			{
				return string.Format("{0:N2}GB", (double)sum / OneGB);
			}
			if(sum >= OneMB)
			{
				return string.Format("{0:N2}MB", (double)sum / OneMB);
			}
			if(sum >= OneKB)
			{
				return string.Format("{0:N2}KB", (double)sum / OneKB);
			}
			return string.Format("{0}B", sum);
		}

		/// <summary>
		///     用法：
		///     Replace( "{0} = ({x},{y}) ", new Point(99,88), "Point" )
		///     如果格式化时遇到不存在的键，则抛出异常
		/// </summary>
		public static string Format(string str, object dict, params object[] args)
		{
			Global.Assert(dict != null);

			return Regex.Replace(str, @"{(\w+)}",
				o =>
				{
					var key = o.Groups[1].Value;
					var value = 0;
					if(int.TryParse(key, out value))
					{
						return args[value].ToString();
					}
					return dict.GetValueEx<string>(key);
				});
		}

		public static string FormatList(IEnumerable list, bool elementAtNewLine = false)
		{
			return FormatEnumerable(list, elementAtNewLine);
		}

		public static string ReplaceSpecialToken(string str, out bool changed)
		{
			changed = false;
			if(string.IsNullOrEmpty(str))
			{
				return str;
			}

			SpecialTokenApplied = false;
			str = regex.Replace(str, matchEvaluator);
			changed = SpecialTokenApplied;
			return str;
		}

		public static string ToHexString(byte[] bytes)
		{
			if(bytes == null || bytes.Length == 0)
			{
				return "";
			}
			var str = "";
			for(var i = 0; i < bytes.Length; ++i)
			{
				str += string.Format("{0:X2}", bytes[i] & 0x00FF).ToLower();
			}
			return str;
		}

		/// <summary>
		///     长度判断
		/// </summary>
		public static bool ValidateLength(string text, int min, int max)
		{
			/*if(text.Length < min)
			{
				return false;
			}*/
			var length = 0;
			foreach(var ch in text)
			{
				if('a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || '0' <= ch && ch <= '9')
				{
					length += 1;
				}
				else
				{
					length += 2;
				}
			}
			return length >= min && length <= max;
		}

		/// <summary>
		///     判断是否保护特殊字符
		/// </summary>
		public static bool CheckSpecialCharacters(string text)
		{
			var regExp = new Regex("[\\'\\\"\\%\\;\\,\\.\t\n\r\\:\\*\\ ]");
			return !regExp.IsMatch(text);
		}

		public static bool CheckMd5(string md5)
		{
			var match = Regex.Match(md5, "^[0-9a-fA-F]{32}$");
			return match.Success;
		}

		private static string FormatEnumerable(IEnumerable enumerable, bool elementAtNewLine = false)
		{
			if(enumerable is string)
			{
				return enumerable as string;
			}

			var first = true;
			var str = "[";

			foreach(var item in enumerable)
			{
				if(first)
				{
					first = false;
				}
				else
				{
					str += ",";
					if(elementAtNewLine)
					{
						str += "\n";
					}
				}

				if(item is IEnumerable)
				{
					str += FormatEnumerable(item as IEnumerable);
				}
				else
				{
					str += item != null ? item.ToString() : null;
				}
			}
			return str + "]";
		}

		private static string ReplaceSpecialToken(Match match)
		{
			/*DataRoleRun role = RoleMan.Instance != null ? RoleMan.Instance.GetRole() : null;
			if (match.Value == "{playername}")
			{
				SpecialTokenApplied = true;
				return role != null ? role.name : "XXX";
			}
			if (match.Value.StartsWith("{sex:", StringComparison.Ordinal))
			{
				var options = match.Value.Substring(5, match.Value.Length - 6).Split('/');
				if (options.Length == 2)
				{
					var sex = role != null ? role.GenderSignal : "m";
					if (sex != null) SpecialTokenApplied = true;
					if (sex == "m")
						return options[0];
					if (sex == "f")
						return options[1];
				}
			}*/
			return match.Value;
		}
	}
}