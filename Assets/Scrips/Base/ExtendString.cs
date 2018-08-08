using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kernel.Lang.Extension
{
	public static class ExtendString
	{
		public static string MakeArgument(this IEnumerable<string> arr)
		{
			var enumerable = arr as string[] ?? arr.ToArray();
			if (arr == null || !enumerable.Any())
			{
				return string.Empty;
			}
			var args = enumerable.Where(x => x != null).Select(x => x.UnixLike()).ToArray();
			return string.Join(" ", args);
		}

		public static bool ArraysAreEqual(this IEnumerable<string> array1, IEnumerable<string> array2)
		{
			var arr1 = array1 as string[] ?? array1.ToArray();
			var arr2 = array2 as string[] ?? array2.ToArray();
			if (arr1.Length != arr2.Length)
			{
				return false;
			}
			Array.Sort(arr1);
			Array.Sort(arr2);
			for (var i = 0; i < arr1.Length; ++i)
			{
				if (arr1[i] != arr2[i])
				{
					return false;
				}
			}
			return true;
		}

		public static string UnixLike(this string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			return str.Replace('\\', '/');
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}

		public static string Join<T>(this string separator, IEnumerable<T> collection)
		{
			return separator.Join(collection, _ => true);
		}

		public static string Join<T>(this string separator, IEnumerable<T> collection, Func<T, bool> collectable)
		{
			StringBuilder sbText = null;
			var iter = collection.GetEnumerator();
			while (iter.MoveNext())
			{
				var item = iter.Current;
				if (collectable(item))
				{
					sbText = new StringBuilder(128);
					sbText.Append(iter.Current);
					break;
				}
			}

			while (iter.MoveNext())
			{
				var item = iter.Current;
				if (collectable(item))
				{
					sbText.Append(separator);
					sbText.Append(iter.Current);
				}
			}

			return null != sbText ? sbText.ToString() : string.Empty;
		}

		public static bool ReversedEquals(this string lhs, string rhs, int deltaIndex = 0)
		{
			if (lhs == rhs)
			{
				return true;
			}

			if (null == lhs || null == rhs)
			{
				return false;
			}

			var count = lhs.Length;
			if (count != rhs.Length)
			{
				return false;
			}

			for (var i = count - deltaIndex - 1; i >= 0; --i)
			{
				if (lhs[i] != rhs[i])
				{
					return false;
				}
			}

			return true;
		}

		public static int ReversedCompareTo(this string lhs, string rhs, int deltaIndex = 0)
		{
			if (lhs == rhs)
			{
				return 0;
			}
			if (null == lhs)
			{
				return -1;
			}
			if (null == rhs)
			{
				return 1;
			}

			var count = lhs.Length;
			if (count < rhs.Length)
			{
				return -1;
			}
			if (count > rhs.Length)
			{
				return 1;
			}

			for (var i = count - deltaIndex - 1; i >= 0; --i)
			{
				var a = lhs[i];
				var b = rhs[i];
				if (a < b)
				{
					return -1;
				}
				if (a > b)
				{
					return 1;
				}
			}

			return 0;
		}

		public static string TrimExtension(this string path)
		{
			var ext = Path.GetExtension(path);
			if (ext != null)
			{
				return TrimRight(path, ext);
			}
			return path;
		}

		public static string TrimLeft(this string str, string left)
		{
			if (str == null)
			{
				return null;
			}
			if (str.IndexOf(left, StringComparison.Ordinal) == 0)
			{
				return str.Substring(left.Length);
			}
			return str;
		}

		public static string TrimRight(this string str, string left)
		{
			if (str == null)
			{
				return null;
			}
			var lastIndexOf = str.LastIndexOf(left, StringComparison.Ordinal);
			if (lastIndexOf != -1 && lastIndexOf == str.Length - left.Length)
			{
				return str.Substring(0, str.Length - left.Length);
			}
			return str;
		}
	}
}