using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Beamable.Server.Editor
{
	[Serializable]
	public class MicroserviceDescriptor : IDescriptor
	{
		public const string ASSEMBLY_FOLDER_NAME = "_assemblyReferences";

		[SerializeField]
		private string _name;
		public string Name
		{
			get => _name;
			set => _name = value;
		}
		public string AttributePath { get; set; }
		public Type Type { get; set; }
		public List<ClientCallableDescriptor> Methods { get; set; }

		public string SourcePath => Path.GetDirectoryName(AttributePath);
		public string HidePath => $"./Assets/~/beamservicehide/{Name}";

		public string CustomCsProjFragmentPath => Path.Combine(SourcePath, "CsProjFragment.xml");
		public string BuildPath => $"./Temp/beam/{(IsGenerator ? "generators/" : String.Empty)}{Name}";
		public string ContainerName => $"{Name}_container";
		public string NugetVolume => $"beamable_microservice_nuget_data"; // TODO: do we need to enter the name here? Does it need to be container specific? I don't think so...
		public string ImageName => Name.ToLower();
		public ServiceType ServiceType => ServiceType.MicroService;
		public bool HasValidationError { get; set; }
		public bool HasValidationWarning { get; set; }
		public bool IsGenerator { get; set; }
		public List<string> FederatedNamespaces { get; set; } = new List<string>();
		public bool IsUsedForFederation => FederatedNamespaces.Count > 0;

		public bool TryGetCustomProjectFragment(out string csProjFragment)
		{
			csProjFragment = "";
			if (File.Exists(CustomCsProjFragmentPath))
			{
				csProjFragment = File.ReadAllText(CustomCsProjFragmentPath);
				return true;
			}

			return false;
		}

	}

	[Serializable]
	public class ClientCallableDescriptor
	{
		public string Path;
		public HashSet<string> Scopes;
		public ClientCallableParameterDescriptor[] Parameters;
	}

	[Serializable]
	public class ClientCallableParameterDescriptor
	{
		public string Name;
		public int Index;
		public Type Type;
	}
}
