using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Api.Sessions
{

	[Serializable]
	public class SessionDeviceOptions
	{
		public List<SessionOption> All => new List<SessionOption>
	  {
		 ClientVersion,
		 new DeviceProperty(),
		 DeviceMemoryOnBoard,
		 GfxDeviceId,
		 GfxDeviceName,
		 GfxVendor,
		 GfxVersion,
		 GfxMemory,
		 GfxShaderLevel,
		 CpuProcessorCount,
		 new CpuProcessorTypeProperty(),
		 Idfa,
		 IosDeviceGeneration,
		 IosSystemVersion,
		 Gaid,
		 new OsVersionProperty()
	  };

		[Serializable]
		public class DeviceProperty : SessionOption
		{
			public override string Key => "device";
			public override GetOptionProperty Get => (args) => SystemInfo.deviceModel.ToString();
			public override bool ForceEnabled => true;
		}

		public ClientVersionProperty ClientVersion = new ClientVersionProperty { UserEnabled = true };
		[Serializable]
		public class ClientVersionProperty : SessionOption
		{
			public override string Key => "client_version";
			public override GetOptionProperty Get => (args) => Application.version;
		}

		public DeviceMemoryOnBoardProperty DeviceMemoryOnBoard;
		[Serializable]
		public class DeviceMemoryOnBoardProperty : SessionOption
		{
			public override string Key => "device_mem_onboard";
			public override GetOptionProperty Get => (args) => SystemInfo.systemMemorySize.ToString();
		}

		public GfxDeviceIdProperty GfxDeviceId;
		[Serializable]
		public class GfxDeviceIdProperty : SessionOption
		{
			public override string Key => "gfx_device_id";
			public override GetOptionProperty Get => (args) => SystemInfo.graphicsDeviceID.ToString();
		}

		public GfxDeviceNameProperty GfxDeviceName;
		[Serializable]
		public class GfxDeviceNameProperty : SessionOption
		{
			public override string Key => "gfx_device_name";
			public override GetOptionProperty Get => (args) => SystemInfo.graphicsDeviceName;
		}

		public GfxVendorProperty GfxVendor;
		[Serializable]
		public class GfxVendorProperty : SessionOption
		{
			public override string Key => "gfx_vendor";
			public override GetOptionProperty Get => (args) => SystemInfo.graphicsDeviceVendor;
		}

		public GfxVersionProperty GfxVersion;
		[Serializable]
		public class GfxVersionProperty : SessionOption
		{
			public override string Key => "gfx_version";
			public override GetOptionProperty Get => (args) => SystemInfo.graphicsDeviceVersion.ToString();
		}

		public GfxMemoryProperty GfxMemory;
		[Serializable]
		public class GfxMemoryProperty : SessionOption
		{
			public override string Key => "gfx_memory";
			public override GetOptionProperty Get => (args) => SystemInfo.graphicsMemorySize.ToString();
		}

		public GfxShaderLevelProperty GfxShaderLevel;
		[Serializable]
		public class GfxShaderLevelProperty : SessionOption
		{
			public override string Key => "gfx_shader_level";
			public override GetOptionProperty Get => (args) => SystemInfo.graphicsShaderLevel.ToString();
		}

		public CpuProcessorCountProperty CpuProcessorCount;
		[Serializable]
		public class CpuProcessorCountProperty : SessionOption
		{
			public override string Key => "cpu_processor_count";
			public override GetOptionProperty Get => (args) => SystemInfo.processorCount.ToString();
		}

		[Serializable]
		public class CpuProcessorTypeProperty : SessionOption
		{
			public override string Key => "cpu_processor_type";
			public override GetOptionProperty Get => (args) => SystemInfo.processorType.ToString();
			public override bool ForceEnabled => true;

		}

		public IdfaProperty Idfa;
		[Serializable]
		public class IdfaProperty : SessionOption
		{
			public override string Key => "idfa";

			public override GetOptionProperty Get => (args) =>
			{
#if UNITY_IOS
            return args.advertisingId;
#else
				return "";
#endif
			};
		}

		public IosDeviceGenerationProperty IosDeviceGeneration;
		[Serializable]
		public class IosDeviceGenerationProperty : SessionOption
		{
			public override string Key => "ios_device_generation";
			public override GetOptionProperty Get => (args) =>
			{
#if UNITY_IOS
            return UnityEngine.iOS.Device.generation.ToString();
#else
				return "";
#endif
			};
		}

		public IosSystemVersionProperty IosSystemVersion;
		[Serializable]
		public class IosSystemVersionProperty : SessionOption
		{
			public override string Key => "ios_system_version";
			public override GetOptionProperty Get => (args) =>
			{
#if UNITY_IOS
            return UnityEngine.iOS.Device.systemVersion.ToString();
#else
				return "";
#endif
			};
		}

		public GaidProperty Gaid;
		[Serializable]
		public class GaidProperty : SessionOption
		{
			public override string Key => "gaid";

			public override GetOptionProperty Get => (args) =>
			{
#if UNITY_ANDROID
            return args.advertisingId;
#else
				return "";
#endif
			};
		}

		[Serializable]
		public class OsVersionProperty : SessionOption
		{
			public override string Key => "osversion";
			public override bool ForceEnabled => true;

			public override GetOptionProperty Get => (args) =>
			{
#if USE_STEAMWORKS
            return "Steam";
#else
				return SystemInfo.operatingSystem.ToString();
#endif
			};
		}
	}

	public delegate string GetOptionProperty(SessionStartRequestArgs args);

	[Serializable]
	public abstract class SessionOption
	{
		public abstract string Key { get; }
		public abstract GetOptionProperty Get { get; }
		public bool UserEnabled;
		public virtual bool ForceEnabled { get; }
		public virtual bool DefaultEnabled { get; }
		public bool IsEnabled => ForceEnabled || UserEnabled;
	}


}
