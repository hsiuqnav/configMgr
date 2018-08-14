using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Kernel.Util
{
	public static class Global
	{
		public static void Assert(bool exp, string msg, params object[] args)
		{
			if(exp == false)
			{
				Logger.Fatal(msg + ": " + GetCaller(), args);
				throw new KernelException(KernelResult.ASSERT);
			}
		}

		public static void Assert(bool exp, string msg)
		{
			if(exp == false)
			{
				Logger.Fatal(msg + ": " + GetCaller());
				throw new KernelException(KernelResult.ASSERT);
			}
		}

		public static void Assert(bool exp)
		{
			if(exp == false)
			{
				Logger.Fatal("assert: " + GetCaller());
				throw new KernelException(KernelResult.ASSERT);
			}
		}

		public static void Assert(bool exp, KernelResult error)
		{
			if(exp == false)
			{
				Logger.Fatal("assert: {0}: {1}", error, GetCaller());
				throw new KernelException(error);
			}
		}

		public static void Assert(KernelResult result, params object[] info)
		{
			if(result != KernelResult.SUCCES)
			{
				Logger.Fatal("assert: {0}: {2} {1}", result, GetCaller(), info.Length > 0 ? StringUtil.FormatList(info) : "");
				throw new KernelException(result, info);
			}
		}

		public static void Throw(KernelResult result, params object[] args)
		{
			throw new KernelException(result, args);
		}

		public static string GetCaller()
		{
			StackTrace st = new StackTrace(true);
			StackFrame sf = st.GetFrame(2);

			if(sf == null)
				return "";
			else
				return string.Format("{0} {1}:{2}", sf.GetMethod(), sf.GetFileName(), sf.GetFileLineNumber());
		}

		public static string FormatException(Exception exception, bool withStack)
		{
			var exType = exception.GetType().Name;
			string exMsg;
			if(withStack)
			{
				string stack = FormatStack(exception);
				exMsg = exception is KernelException ? (exception as KernelException).StackMessage : exception.Message;
				if(!string.IsNullOrEmpty(stack))
					exMsg = exMsg + "\n" + stack;
			}
			else
			{
				exMsg = exception.Message;
			}
			return string.Format("({0}) {1}", exType, exMsg);
		}

		// frame = 0 为调用者所在帧
		public static string GetStack(int skipFrames = 0)
		{
			return FormatStack(new StackTrace(skipFrames, true));
		}

		public static string FormatStack(Exception exception)
		{
			return FormatStack(new StackTrace(exception, true));
		}

		private static string FormatStack(StackTrace st)
		{
			var sb = GetLocalStringBuilder(512);

			for(int i = 0; i < st.FrameCount; ++i)
			{
				if(i != 0)
					sb.AppendFormat("\n");

				StackFrame sf = st.GetFrame(i);
				if(sf == null)
					continue;

				MethodBase method = sf.GetMethod();
				if(method == null)
				{
					sb.Append("  unknown method");
				}
				else
				{
					sb.Append("  ");
					if(method.ReflectedType != null)
						sb.AppendFormat("{0}:{1}", method.ReflectedType.Name, method.Name);
					else
						sb.Append(method.Name);

					sb.Append("(");
					foreach(var param in method.GetParameters())
					{
						if(param.Position != 0)
							sb.Append(", ");
						if(param.IsOut)
							sb.Append("out ");
						sb.Append(param.ParameterType.Name);
					}
					sb.Append(")");
				}

				string filename = sf.GetFileName();
				if(filename != null)
				{
					sb.AppendFormat("  in {0}:{1}", filename, sf.GetFileLineNumber());
				}
			}
			return sb.ToString();
		}

		private readonly static StringBuilder mainThreadStringBuilder = new StringBuilder();
		private static StringBuilder GetLocalStringBuilder(int capacity)
		{
			if(Thread.CurrentThread.ManagedThreadId == 1)
			{
				mainThreadStringBuilder.Length = 0;
				return mainThreadStringBuilder;
			}
			return new StringBuilder(capacity);
		}
	}
}
