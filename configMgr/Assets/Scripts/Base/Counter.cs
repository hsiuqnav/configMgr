using System;
using Kernel.Lang.Extension;

namespace Kernel.Engine
{
	public class Counter : ICounter<float>
	{
		public Counter(float total = float.PositiveInfinity)
		{
			Redefine(total);
		}

		public float CurrentNormalized
		{
			get
			{
				if (IsPositiveFinity())
				{
					return Math.Min(Total, Current) / Total;
				}
				return 1.0f;
			}
		}

		public float Remain
		{
			get
			{
				if (IsPositiveFinity())
				{
					return Math.Max(0.0f, Total - Current);
				}
				return 0.0f;
			}
		}

		public float RemainNormalized
		{
			get
			{
				if (IsPositiveFinity())
				{
					return Remain / Total;
				}
				return 0.0f;
			}
		}

		public float Current
		{
			get;
			private set;
		}

		public float Total
		{
			get;
			private set;
		}

		public void Delay(float extra)
		{
			if (extra > 0)
			{
				Total += extra;
			}
		}

		public void Exceed()
		{
			Current = Total;
		}

		//当增加到目标值后，返回true，否则返回false
		public bool Increase(float delta)
		{
			if (Current + delta < Total)
			{
				Current += delta;
			}
			else
			{
				Current = Total;
			}
			return IsExceed();
		}

		public void Infinity()
		{
			Total = float.PositiveInfinity;
			Reset();
		}

		public bool IsExceed()
		{
			return Current >= Total;
		}

		public bool IsFinity()
		{
			return Total < float.PositiveInfinity;
		}

		public bool IsInfinity()
		{
			return float.IsPositiveInfinity(Total);
		}

		public bool IsNotExceed()
		{
			return Current < Total;
		}

		public bool IsPositiveFinity()
		{
			return 0.0f < Total && Total < float.PositiveInfinity;
		}

		public bool IsZero()
		{
			return Total.IsZero();
		}

		public void Redefine(float total)
		{
			if (total < 0.0f)
			{
				Total = float.PositiveInfinity;
			}
			else
			{
				Total = total;
			}
			Reset();
		}

		public void Reset()
		{
			Current = 0.0f;
		}

		public void Zero()
		{
			Total = 0.0f;
			Reset();
		}
	}
}