using System;

namespace Kernel.Util
{
	public static class TimeUtil
	{
		private static readonly DateTime CTIME_BEGIN;

		static TimeUtil()
		{
			CTIME_BEGIN = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
		}

		public static long LocalMachineTime()
		{
			return UtcDateToCTime(DateTime.UtcNow);
		}

		public static double LocalMachineTimeDoubleFloatingPoint()
		{
			return DateToCTimeDoubleFloatingPoint(DateTime.UtcNow);
		}

		/// <summary>
		/// 从C标准库的时间，即自1970年1月1日起的秒数，转UTC DateTime 
		/// </summary>
		public static DateTime CTimeToUtcDate(long time)
		{
			if (time == 0L)
			{
				return CTIME_BEGIN;
			}

			if (time < 0L)
			{
				Logger.Fatal("请找xxoo同学： time < 0");
				time = 0L;
			}

			return DateTime.SpecifyKind(CTIME_BEGIN + new TimeSpan(time * TimeSpan.TicksPerSecond), DateTimeKind.Utc);
		}

		public static long LocalDateToCTime(DateTime time)
		{
			var offset = new TimeSpan(28800 * TimeSpan.TicksPerSecond);
			if (time <= CTIME_BEGIN + offset)
				return 0;
			return UtcDateToCTime(time - offset);
		}

		public static long UtcDateToCTime(DateTime time)
		{
			if (time.Ticks == 0)
			{
				return 0L;
			}

			var ts = time.Subtract(CTIME_BEGIN);
			var ctime = (long)ts.TotalSeconds;

			if (ctime < 0L)
			{
				Logger.Fatal("试图转换一个早于1970年1月1日的时间：" + time.ToString("yyyy-MM-dd HH:mm:ss"));
				ctime = 0L;
			}

			return ctime;
		}

		// 返回整数部分与C标准库一致，即自1970年1月1日起的秒数。同时带较高精度（与C# DateTime精度一致）的小数部分。
		// 其中0为特殊值
		public static double DateToCTimeDoubleFloatingPoint(DateTime time)
		{
			if (time.Ticks == 0)
			{
				return 0.0f;
			}

			var ts = time.Subtract(CTIME_BEGIN);
			var ctime = ts.TotalSeconds;

			if (ctime < 0.0)
			{
				Logger.Fatal("试图转换一个早于1970年1月1日的时间：" + time.ToString("yyyy-MM-dd HH:mm:ss"));
				ctime = 0.0;
			}
			return ctime;
		}
	}
}