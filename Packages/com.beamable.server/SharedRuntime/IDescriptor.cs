using System;

namespace Beamable.Server.Editor
{
	public interface IDescriptor
	{
		string Name { get; }
		string AttributePath { get; }
		Type Type { get; set; }

		string ContainerName { get; }
		string ImageName { get; }
		ServiceType ServiceType { get; }
		bool HasValidationError { get; }
		bool HasValidationWarning { get; }
	}

	public enum ServiceType
	{
		MicroService,
		StorageObject
	}
}
