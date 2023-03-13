using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Serialization;
using System;
using System.Collections.Generic;
using static Beamable.Common.Constants.Features.Services;

namespace Beamable.Server.Editor.ManagerClient
{
	public class MicroserviceManager
	{
		public const string SERVICE = "/basic/beamo";

		public IPlatformRequester Requester { get; }

		public MicroserviceManager(IPlatformRequester requester)
		{
			Requester = requester;
		}

		public Promise<ServiceManifest> GetCurrentManifest()
		{
			return Requester.Request<GetManifestResponse>(Method.GET, $"{SERVICE}/manifest/current", "{}")
			   .Map(res => res.manifest)
			   .RecoverFrom40x(ex => new ServiceManifest());
		}

		[Obsolete(Constants.Commons.OBSOLETE_WILL_BE_REMOVED)]
		public Promise<GetLogsResponse> GetLogs(MicroserviceDescriptor service, string filter = null)
		{
			return Requester.RequestJson<GetLogsResponse>(Method.POST, $"{SERVICE}/logs", new GetLogsRequest
			{
				serviceName = service.Name,
				filter = filter
			});
		}

		[Obsolete(Constants.Commons.OBSOLETE_WILL_BE_REMOVED)]
		public Promise<ServiceManifest> GetManifest(long id)
		{
			return Requester.Request<GetManifestResponse>(Method.GET, $"{SERVICE}/manifest?id={id}")
			.Map(res => res.manifest);
		}

		public Promise<List<ServiceManifest>> GetManifests()
		{
			return Requester.Request<GetManifestsResponse>(Method.GET, $"{SERVICE}/manifests")
			   .Map(res => res.manifests)
			   .RecoverFrom40x(err => new List<ServiceManifest>());
		}

		public Promise<Unit> Deploy(ServiceManifest manifest)
		{
			var post = new PostManifestRequest
			{
				comments = manifest.comments,
				manifest = manifest.manifest,
				storageReferences = manifest.storageReference
			};
			return Requester.Request<EmptyResponse>(Method.POST, $"{SERVICE}/manifest", post).ToUnit();
		}

		public Promise<GetStatusResponse> GetStatus()
		{
			return Requester.Request<GetStatusResponse>(Method.GET, $"{SERVICE}/status")
							.RecoverFrom40x(err => new GetStatusResponse
							{
								isCurrent = false,
								services = new List<ServiceStatus>()
							});
		}

	}

	[System.Serializable]
	public class GetManifestResponse
	{
		public ServiceManifest manifest;
	}
	[System.Serializable]
	public class GetManifestsResponse
	{
		public List<ServiceManifest> manifests;
	}
	[System.Serializable]
	public class PostManifestRequest
	{
		public string comments;
		public List<ServiceReference> manifest;
		public List<ServiceStorageReference> storageReferences;
	}

	[System.Serializable]
	public class ServiceManifest
	{
		public long id;
		public long created;
		public List<ServiceReference> manifest = new List<ServiceReference>();
		public List<ServiceStorageReference> storageReference = new List<ServiceStorageReference>();
		public long createdByAccountId;
		public string comments;
	}

	[System.Serializable]
	public class ServiceReference
	{
		public string serviceName;
		public string checksum;
		public bool enabled;
		public bool archived;
		public string imageId;
		public string imageCpuArch;
		public string templateId;
		public string comments;
		public List<ServiceDependency> dependencies;
		public long containerHealthCheckPort = HEALTH_PORT;
	}


	[System.Serializable]
	public class ServiceStorageReference
	{
		public string id;
		public string storageType;
		public bool enabled;
		public bool archived;
		public string templateId;
		public string checksum;
	}

	[System.Serializable]
	public class ServiceDependency
	{
		public string storageType;
		public string id;
	}

	[System.Serializable]
	public class GetStatusResponse
	{
		public bool isCurrent;
		public List<ServiceStatus> services;
	}

	[System.Serializable]
	public class ServiceStatus
	{
		public string serviceName;
		public string imageId;
		public bool running;
		public bool isCurrent;
	}

	[System.Serializable]
	public class GetLogsRequest : JsonSerializable.ISerializable
	{
		public string serviceName;
		public string filter;
		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize(nameof(serviceName), ref serviceName);
			if (!string.IsNullOrEmpty(filter))
			{
				s.Serialize(nameof(filter), ref filter);
			}
		}
	}

	[System.Serializable]
	public class GetLogsResponse
	{
		public string serviceName;
		public List<LogMessage> logs;
	}

	[System.Serializable]
	public class LogMessage
	{
		public string level;
		public long timestamp;
		public string message;
	}

	//   case class GetStatusResponse(
	//      services: Seq[ServiceStatus],
	//   isCurrent: Boolean // this is the magic field that says, "Are all the services where they're supposed to be as declared in the manifest?"
	//   ) extends NetworkSerializable
	//
	//   case class ServiceStatus(
	//      serviceName: String,
	//      running: Boolean, // this probably needs to be an enum of STARTING, STOPPING, RUNNING
	//      imageId: String, // this is the image of the task def
	//      isCurrent: Boolean // this is the magic field that says, "Is this service state what its supposed to be in the manifest?"
	//   )

}
