using Kernel.Lang.Extension;

namespace Kernel.Engine
{
	public partial struct Color
	{
		public static implicit operator UnityEngine.Color(Color c)
		{
			return new UnityEngine.Color(c.r, c.g, c.b, c.a);
		}

		public static implicit operator Color(UnityEngine.Color c)
		{
			return new Color(c.r, c.g, c.b, c.a);
		}

		public static bool operator ==(Color c1, UnityEngine.Color c2)
		{
			return c1.r.EqualsEx(c2.r) && c1.g.EqualsEx(c2.g) && c1.b.EqualsEx(c2.b) && c1.a.EqualsEx(c2.a);
		}

		public static bool operator !=(Color c1, UnityEngine.Color c2)
		{
			return !(c1 == c2);
		}

		public static bool operator ==(UnityEngine.Color c1, Color c2)
		{
			return c1.r.EqualsEx(c2.r) && c1.g.EqualsEx(c2.g) && c1.b.EqualsEx(c2.b) && c1.a.EqualsEx(c2.a);
		}

		public static bool operator !=(UnityEngine.Color c1, Color c2)
		{
			return !(c1 == c2);
		}
	}
}