using UnityEngine;

namespace Kernel
{
	public abstract class UnityEnginePlatformModule : PlatformModule
	{
		protected string deviceCode;
		private int processorCount;
		private string operatingSystem;
		private string deviceModel;

		public override string ClientVersion
		{
			get
			{
				return Application.version;
			}
		}

		public override int ProcessorCount
		{
			get
			{
				if (0 == processorCount)
				{
					processorCount = SystemInfo.processorCount;
				}
				return processorCount;
			}
		}

		public override string OperatingSystem
		{
			get
			{
				if(!string.IsNullOrEmpty(operatingSystem))
				{
					return operatingSystem;
				}

				operatingSystem = SystemInfo.operatingSystem;
				return operatingSystem;
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

				deviceCode = SystemInfo.deviceUniqueIdentifier;

				return deviceCode;
			}
		}

		public override string DeviceModel
		{
			get
			{
				if(!string.IsNullOrEmpty(deviceModel))
				{
					return deviceModel;
				}

				deviceModel = SystemInfo.deviceModel;
				return deviceModel;
			}
		}

		public override void InitPlatformSettings()
		{
			Application.targetFrameRate = 30;
		}

		public override void InitPlatformInfoAndDevicesInfo()
		{
			PrintSystemInfo();
		}

		public override void QuitGame()
		{
			Application.Quit();
		}

		private void PrintSystemInfo()
		{
			var sysInfo = "deviceModel:" + SystemInfo.deviceModel +
			              "\r\ndeviceName:" + SystemInfo.deviceName +
			              "\r\ndeviceType:" + SystemInfo.deviceType +
			              "\r\ndeviceUniqueIdentifier:" + SystemInfo.deviceUniqueIdentifier +
			              "\r\ngraphicsDeviceID:" + SystemInfo.graphicsDeviceID +
			              "\r\ngraphicsDeviceName:" + SystemInfo.graphicsDeviceName +
			              "\r\ngraphicsDeviceType:" + SystemInfo.graphicsDeviceType +
			              "\r\ngraphicsDeviceVendor:" + SystemInfo.graphicsDeviceVendor +
			              "\r\ngraphicsDeviceVendorID:" + SystemInfo.graphicsDeviceVendorID +
			              "\r\ngraphicsDeviceVersion:" + SystemInfo.graphicsDeviceVersion +
			              "\r\ngraphicsMemorySize:" + SystemInfo.graphicsMemorySize +
			              "\r\ngraphicsMultiThreaded:" + SystemInfo.graphicsMultiThreaded +
			              "\r\ngraphicsShaderLevel:" + SystemInfo.graphicsShaderLevel +
			              "\r\nmaxTextureSize:" + SystemInfo.maxTextureSize +
			              "\r\nnpotSupport:" + SystemInfo.npotSupport +
			              "\r\noperatingSystem:" + SystemInfo.operatingSystem +
			              "\r\nprocessorCount:" + SystemInfo.processorCount +
			              "\r\nprocessorType:" + SystemInfo.processorType +
			              "\r\nsupportedRenderTargetCount:" + SystemInfo.supportedRenderTargetCount +
			              "\r\nsupports3DTextures:" + SystemInfo.supports3DTextures +
			              "\r\nsupportsAccelerometer:" + SystemInfo.supportsAccelerometer +
			              "\r\nsupportsComputeShaders:" + SystemInfo.supportsComputeShaders +
			              "\r\nsupportsGyroscope:" + SystemInfo.supportsGyroscope +
			              "\r\nsupportsImageEffects:" + SystemInfo.supportsImageEffects +
			              "\r\nsupportsInstancing:" + SystemInfo.supportsInstancing +
			              "\r\nsupportsLocationService:" + SystemInfo.supportsLocationService +
			              "\r\nsupportsRenderToCubemap:" + SystemInfo.supportsRenderToCubemap +
			              "\r\nsupportsShadows:" + SystemInfo.supportsShadows +
			              "\r\nsupportsSparseTextures:" + SystemInfo.supportsSparseTextures +
			              "\r\nsupportsVibration:" + SystemInfo.supportsVibration +
			              "\r\nsystemMemorySize:" + SystemInfo.systemMemorySize;

			sysInfo +=
				"\r\ncurrentResolution:" + Screen.currentResolution +
				"\r\nwidth:" + Screen.width +
				"\r\nheight:" + Screen.height +
				"\r\ndpi:" + Screen.dpi;

			Logger.Info(sysInfo);
		}
	}
}
