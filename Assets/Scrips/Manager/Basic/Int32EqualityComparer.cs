using System;
using System.Collections.Generic;

namespace Kernel
{
	public class Int32EqualityComparer : Singleton<Int32EqualityComparer>, IEqualityComparer<Int32>
	{
		public bool Equals(int x, int y)
		{
			return x == y;
		}

		public int GetHashCode(int obj)
		{
			return obj;
		}
	}
}
