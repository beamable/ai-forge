using System;
using UnityEngine;

namespace Beamable.Server.Editor
{
	[System.Serializable]
	public class StorageObjectDescriptor : IDescriptor
	{
		[SerializeField]
		private string _name;
		public string Name
		{
			get => _name;
			set => _name = value;
		}
		public string AttributePath { get; set; }
		public Type Type { get; set; }
		public string ContainerName => $"db_{Name}_storage";
		public string ImageName => "mongo:latest";
		public ServiceType ServiceType => ServiceType.StorageObject;
		public bool HasValidationError { get; set; }
		public bool HasValidationWarning { get; set; }

		public string DataVolume => $"beamable_storage_{Name}_data";
		public string FilesVolume => $"beamable_storage_{Name}_files";

		public string LocalToolContainerName => $"tool_{Name}_storage";
		public string ToolImageName => $"mongo-express:latest";

		public bool IsPublishFeatureDisabled()
		{
			return false;
		}
	}
}
