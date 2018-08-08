using System;
using System.Text;

namespace Kernel.Util
{
	public class KernelException : Exception
	{
		private KernelResult result;
		private object[] args;

		public KernelException(KernelResult result, params object[] args)
		{
			this.result = result;
			this.args = args;
		}

		public override string Message
		{
			get
			{
				var localeKey = LocaleKey;
				string extra = "";
				foreach(var arg in args)
				{
					extra += "\n" + arg;
				}
				return string.Format("Kernel Exception {0}{1}", result, extra);
			}
		}

		public string StackMessage
		{
			get
			{
				StringBuilder sb = new StringBuilder(Message);
				foreach(var arg in args)
				{
					if(arg is Exception)
					{
						sb.AppendLine(Global.FormatException(arg as Exception, true));
					}
				}
				return sb.ToString();
			}
		}

		private string LocaleKey
		{
			get { return "kernel.result." + (int)result; }
		}
	}
}
