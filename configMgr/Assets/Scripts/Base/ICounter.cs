namespace Kernel.Engine
{
	public interface ICounter<T>
	{
		T Remain
		{
			get;
		}

		T Current
		{
			get;
		}

		T Total
		{
			get;
		}

		void Delay(T extra);

		void Exceed();

		//当增加到目标值后，返回true，否则返回false
		bool Increase(T delta);

		void Infinity();

		bool IsExceed();

		bool IsFinity();

		bool IsInfinity();

		bool IsNotExceed();

		bool IsPositiveFinity();

		bool IsZero();

		void Redefine(T total);

		void Reset();

		void Zero();
	}
}