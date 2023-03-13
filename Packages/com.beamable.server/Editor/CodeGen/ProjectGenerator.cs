using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Beamable.Server.Editor.CodeGen
{
	public class ProjectGenerator
	{
		public MicroserviceDependencies Dependencies { get; }

		public MicroserviceDescriptor Descriptor { get; }
		/*
		 * <Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
	  <DefineConstants>DB_MICROSERVICE</DefineConstants>
	  <OutputType>Exe</OutputType>
	  <TargetFramework>netcoreapp3.0</TargetFramework>
	</PropertyGroup>

	<ItemGroup>
	  <PackageReference Include="Newtonsoft.Json" Version="12.0.3" />
	  <Reference Include="BeamableMicroserviceBase">
		<HintPath>/src/obj/Release/netcoreapp3.0/BeamableMicroserviceBase.dll</HintPath>
	  </Reference>
	</ItemGroup>
  </Project>

		 */


		public ProjectGenerator(MicroserviceDescriptor descriptor, MicroserviceDependencies dependencies)
		{
			Dependencies = dependencies;
			Descriptor = descriptor;
		}

		string GetDllDependencyString(PluginImporter dll)
		{
			var name = Path.GetFileName(dll.assetPath);
			var nameWithoutExt = Path.GetFileNameWithoutExtension(dll.assetPath);
			return $@"      <Reference Include=""{nameWithoutExt}"">
                  <HintPath>./libdll/{name}</HintPath>
               </Reference>";
		}

		string GetDllDependenciesString()
		{
			if (Dependencies.DllsToCopy == null || Dependencies.DllsToCopy.Count == 0)
			{
				return "";
			}

			return $@"<ItemGroup>
         {string.Join(Environment.NewLine, Dependencies.DllsToCopy.Select(GetDllDependencyString))}
            </ItemGroup>";
		}

		string GetCustomFragmentString()
		{
			if (Descriptor.TryGetCustomProjectFragment(out var fragment))
			{
				return fragment;
			}

			return "";
		}
		public string GetString()
		{
			// <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>

			var text = $@"<Project Sdk=""Microsoft.NET.Sdk"">
            <PropertyGroup>
               <DefineConstants>DB_MICROSERVICE;BEAMABLE_MICROSERVICE;BEAMABLE_IGNORE_MONGO_MOCKS</DefineConstants>
               <OutputType>Exe</OutputType>
               <TargetFramework>net6.0</TargetFramework>
            </PropertyGroup>

            <PropertyGroup>
               <NoWarn>1591</NoWarn>
               <DocumentationFile>serviceDocs.xml</DocumentationFile>
            </PropertyGroup>

{GetCustomFragmentString()}
            <ItemGroup>
               <Reference Include=""BeamableMicroserviceBase"">
                  <HintPath>/app/BeamableMicroserviceBase.dll</HintPath>
<SpecificVersion>False</SpecificVersion>
               </Reference>
               <Reference Include=""Beamable.Common"">
                  <HintPath>/src/lib/Beamable.Common.dll</HintPath>
<SpecificVersion>False</SpecificVersion>
               </Reference>
               <Reference Include=""Beamable.Server"">
                  <HintPath>/src/lib/Beamable.Server.dll</HintPath>
<SpecificVersion>False</SpecificVersion>
               </Reference>
               <Reference Include=""Beamable.Server.Common"">
                  <HintPath>/src/lib/Beamable.Server.Common.dll</HintPath>
<SpecificVersion>False</SpecificVersion>
               </Reference>
               <Reference Include=""UnityEngine"">
                  <HintPath>/src/lib/UnityEngine.dll</HintPath>
<SpecificVersion>False</SpecificVersion>
               </Reference>

            </ItemGroup>
            {GetDllDependenciesString()}
            </Project>
            ";
			return text;
		}

		public void Generate(string filePath)
		{
			var content = GetString();
			File.WriteAllText(filePath, content);
		}
	}
}
