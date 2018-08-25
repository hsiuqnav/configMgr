using System.Diagnostics.CodeAnalysis;
using Kernel.Lang.Extension;

// ReSharper disable InconsistentNaming

namespace Kernel.Engine
{
	public partial struct Color
	{
		public float r;
		public float g;
		public float b;
		public float a;
		public static readonly Color white = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		public static readonly Color gray = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		public static readonly Color black = new Color(0f, 0f, 0f, 1f);
		public static readonly Color magenta = new Color(1f, 0f, 1f, 1f);
		public static readonly Color magentaAlpha = new Color(1f, 0f, 1f, 0.297f);
		public static readonly Color yellow = new Color(1f, 1f, 0f, 1f);
		public static readonly Color yellowAlpha = new Color(1f, 1f, 0f, 0.297f);
		public static readonly Color cyan = new Color(0, 1f, 1f, 1f);
		public static readonly Color cyanAlpha = new Color(0, 1f, 1f, 0.297f);
		public static readonly Color red = new Color(1.0f, 0f, 0f, 1f);
		public static readonly Color green = new Color(0.0f, 1f, 0f, 1f);
		public static readonly Color blue = new Color(0.0f, 0f, 1f, 1f);

		public static bool operator ==(Color c1, Color c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(Color c1, Color c2)
		{
			return !(c1 == c2);
		}

		public static implicit operator uint(Color c)
		{
			return ((uint)(c.r * 255) << 24) + ((uint)(c.g * 255) << 16) + ((uint)(c.b * 255) << 8) + (uint)(c.a * 255);
		}

		public static implicit operator Color(uint c)
		{
			return new Color((byte)(c >> 24), (byte)(c >> 16), (byte)(c >> 8), (byte)c);
		}

		public override string ToString()
		{
			return string.Format("({0}, {1}, {2}, {3})", r, g, b, a);
		}

		public override bool Equals(object c)
		{
			if (ReferenceEquals(null, c))
			{
				return false;
			}
			return c is Color && Equals((Color)c);
		}

		public bool Equals(Color other)
		{
			return r.EqualsEx(other.r) && g.EqualsEx(other.g) && b.EqualsEx(other.b) && a.EqualsEx(other.a);
		}

		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = r.GetHashCode();
				hashCode = (hashCode * 397) ^ g.GetHashCode();
				hashCode = (hashCode * 397) ^ b.GetHashCode();
				hashCode = (hashCode * 397) ^ a.GetHashCode();
				return hashCode;
			}
		}

		public Color(byte r1, byte g1, byte b1, byte a1)
		{
			r = r1 / 255f;
			g = g1 / 255f;
			b = b1 / 255f;
			a = a1 / 255f;
		}

		public Color(float r, float g, float b, float a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public Color(float r, float g, float b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			a = 1f;
		}
	}
}