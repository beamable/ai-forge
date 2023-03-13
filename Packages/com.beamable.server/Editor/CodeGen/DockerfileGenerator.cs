using System;
using System.IO;
using System.Linq;
using static Beamable.Common.Constants.Features.Services;

namespace Beamable.Server.Editor.CodeGen
{
	public class DockerfileGenerator
	{
		public MicroserviceDescriptor Descriptor { get; }
		public bool Watch { get; }
		public MicroserviceConfigurationEntry Config { get; }

		private bool DebuggingEnabled = true;
		public const string DOTNET_RUNTIME_DEBUGGING_TOOLS_IMAGE = "mcr.microsoft.com/dotnet/runtime:6.0";
		public const string DOTNET_RUNTIME_IMAGE = "mcr.microsoft.com/dotnet/runtime:6.0-alpine";


#if BEAMABLE_DEVELOPER
      public const string BASE_IMAGE = "beamservice"; // Use a locally built image.
      public static string BASE_TAG => "latest"; // Use a locally built image.
#else
		public const string BASE_IMAGE = "beamableinc/beamservice"; // use the public online image.
		public static string BASE_TAG => BeamableEnvironment.BeamServiceTag;
#endif

		public DockerfileGenerator(MicroserviceDescriptor descriptor, bool includeDebugTools, bool watch)
		{
			DebuggingEnabled = includeDebugTools;
			Descriptor = descriptor;
			Watch = watch;
			Config = MicroserviceConfiguration.Instance.GetEntry(descriptor.Name);
		}

		string GetOpenSshConfigString()
		{
			return $@"
Port 			             2222
ListenAddress 		       0.0.0.0
LoginGraceTime 		    180
Ciphers                  aes128-cbc,3des-cbc,aes256-cbc,aes128-ctr,aes192-ctr,aes256-ctr
MACs                     hmac-sha1,hmac-sha1-96
StrictModes 	       	 yes
SyslogFacility 	       DAEMON
PasswordAuthentication 	 yes
PermitEmptyPasswords 	 no
PermitRootLogin 	       yes
Subsystem sftp internal-sftp

";
		}

		string GetSupervisorDConfigString()
		{
			return $@"
[supervisord]
nodaemon=true
user=root
loglevel=error

[program:${Descriptor.Name}]
command={ (Watch ? "dotnet watch" : $"/usr/bin/dotnet {GetProgramDll()}")}
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0

[program:ssh]
command=/usr/sbin/sshd -D
";
		}

		string WriteToFile(string multiline, string fileName)
		{
			return
			   $@"RUN {string.Join(" && \\\n", multiline.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => $"echo \"{x}\" >> {fileName}"))}";
		}

		string GetDebugLayer()
		{
			if (!DebuggingEnabled) return "";

			//RUN curl -sSL -o /riderdbg https://download.jetbrains.com/rider/ssh-remote-debugging/linux-arm64/jetbrains_debugger_agent_20210604.19.0

			var riderTools = "";
			MicroserviceConfiguration.Instance.RiderDebugTools.DoIfExists(tools =>
			{
				riderTools = $@"
RUN curl -sSL {tools.RiderToolsDownloadUrl} -o ridertemp.zip
RUN mkdir -p /root/.local/share/JetBrains/RiderRemoteDebugger/{tools.RiderVersion}
RUN unzip -q -o ridertemp.zip -d /root/.local/share/JetBrains/RiderRemoteDebugger/{tools.RiderVersion}
";
			});

			return $@"
#inject the debugging tools
RUN apt update && \
    apt install -y unzip curl supervisor openssh-server && \
    curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

{riderTools}

RUN mkdir -p /var/log/supervisor /run/sshd

{WriteToFile(GetSupervisorDConfigString(), "/etc/supervisor/conf.d/supervisord.conf")}
RUN rm -f /etc/ssh/sshd_config
{WriteToFile(GetOpenSshConfigString(), "/etc/ssh/sshd_config")}

RUN echo ""{Config.DebugData.Username}:{Config.DebugData.Password}"" | chpasswd

EXPOSE 80 2222
";
		}

		string GetProgramDll()
		{
			return $"/subapp/{Descriptor.ImageName}.dll";
		}

		string GetEntryPoint()
		{
			if (DebuggingEnabled)
			{
				return @"ENTRYPOINT [""/usr/bin/supervisord"", ""-c"", ""/etc/supervisor/conf.d/supervisord.conf""]";
			}
			else if (Watch)
			{
				return $@"ENTRYPOINT [""dotnet"", ""watch""]";
			}
			else
			{
				return $@"ENTRYPOINT [""dotnet"", ""{GetProgramDll()}""]";
			}
		}

		string ReleaseMode()
		{
			return DebuggingEnabled ? "debug" : "release";
		}

		private string GetWatchDockerFile()
		{
			var text = $@"
FROM {BASE_IMAGE}:{BASE_TAG} AS build-env
RUN dotnet --version
WORKDIR /subapp

COPY {Descriptor.ImageName}.csproj .
RUN cp /src/baseImageDocs.xml .

RUN echo $BEAMABLE_SDK_VERSION > /subapp/.beamablesdkversion

{GetDebugLayer()}

EXPOSE {HEALTH_PORT}
ENV BEAMABLE_SDK_VERSION_EXECUTION={BeamableEnvironment.SdkVersion}
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1
RUN dotnet restore .
{GetEntryPoint()}
";
			return text;
		}

		public string GetString()
		{

			if (Watch)
			{
				return GetWatchDockerFile();
			}

			var text = $@"
# step 1. Build...
FROM {BASE_IMAGE}:{BASE_TAG} AS build-env
WORKDIR /subsrc

COPY {Descriptor.ImageName}.csproj .

#RUN dotnet restore
COPY . .
RUN dotnet publish -c {ReleaseMode()} -o /subapp
RUN echo $BEAMABLE_SDK_VERSION > /subapp/.beamablesdkversion

# step 2. Package using the runtime
FROM {(DebuggingEnabled
			   ? DOTNET_RUNTIME_DEBUGGING_TOOLS_IMAGE
			   : DOTNET_RUNTIME_IMAGE)}
{GetDebugLayer()}

WORKDIR /subapp

EXPOSE {HEALTH_PORT}
COPY --from=build-env /subapp .
COPY --from=build-env /app/baseImageDocs.xml .
ENV BEAMABLE_SDK_VERSION_EXECUTION={BeamableEnvironment.SdkVersion}
{GetEntryPoint()}
";

			return text;
		}


		public void Generate(string filePath)
		{
			var content = GetString();

#if BEAMABLE_DEVELOPER
			Beamable.Common.BeamableLogger.Log($"DOCKER FILE {Descriptor.Name}\n{content}");
#endif

			File.WriteAllText(filePath, content);
		}

	}
}
