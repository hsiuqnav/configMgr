using System;
using System.Net.NetworkInformation;

namespace Kernel
{
	public class UnityEditorPlatformModule : UnityEnginePlatformModule
	{
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

		public override Platform Platform
		{
			get
			{
#if UNITY_EDITOR_OSX
				return Platform.OSX;
#else
				return Platform.WINDOWS;
#endif
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
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
	}
}