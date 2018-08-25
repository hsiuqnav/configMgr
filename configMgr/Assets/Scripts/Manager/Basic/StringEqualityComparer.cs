using System;
using System.Collections.Generic;

namespace Kernel
{
	public class StringEqualityComparer : Singleton<StringEqualityComparer>, IEqualityComparer<string>
	{
		public bool Equals(string x, string y)
		{
			return string.Equals(x, y, StringComparison.Ordinal);
		}

		public int GetHashCode(string obj)
		{
			return obj.GetHashCode();
		}
	}
}
