using System;
using System.Net.NetworkInformation;

namespace Kernel
{
	public class CommandLinePlatformModule : PlatformModule
	{
		private string deviceCode;
		private string deviceModel;

		public override string ClientVersion
		{
			get
			{
				return "1.0";
			}
		}

		public override string DeviceCode
		{
			get
			{
				if(!string.IsNullOrEmpty(deviceCode))
				{
					return deviceCode;
				}

				foreach(var network in NetworkInterface.GetAllNetworkInterfaces())
				{
					if(network.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
					{
						deviceCode = network.GetPhysicalAddress().ToString();
						break;
					}
				}

				return deviceCode;
			}
		}

		public override string DeviceModel
		{
			get
			{
				return "Command Line";
			}
		}

		public override string OperatingSystem
		{
			get
			{
				return "Unknown OS";
			}
		}

		public override Platform Platform
		{
			get
			{
				return Platform.CONSOLE;
			}
		}

		public override int ProcessorCount
		{
			get
			{
				return 1;
			}
		}

		public override bool CheckStorage(long byteSize)
		{
			return true;
		}

		public override bool ApplyPatch(string oldFile, string newFile, string patchFile)
		{
			throw new NotImplementedException();
		}

		public override void QuitGame()
		{
			Environment.Exit(0);
		}
	}
}