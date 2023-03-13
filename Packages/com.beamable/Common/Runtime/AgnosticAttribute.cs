using System;
using System.Runtime.CompilerServices;

namespace Beamable
{

	/// <summary>
	/// This type defines those types with a %SourcePath
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.AgnosticAttribute script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IHasSourcePath
	{
		string SourcePath { get; }
	}

	/// <summary>
	/// This type defines the field attribute that marks a %Beamable %ContentObject field
	/// as compatible with %Beamable %Microservices.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Server.Microservice script reference
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code">Content - Agnostic</a> documentation
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
	[Obsolete("The agnostic attribute is used to copy entire source files into Microservice builds. However, this leads to unstable dependency management. Instead, you should put shared code into a custom assembly definition, and reference that assembly from the Microservice assembly. Beamable offers a standard common assembly by default in Assets/Beamable/Common.")]
	public class AgnosticAttribute : Attribute, IHasSourcePath
	{
		public Type[] SupportTypes { get; }
		public string SourcePath { get; }
		public string MemberName { get; }

		public AgnosticAttribute(Type[] supportTypes = null, [CallerFilePath] string sourcePath = "", [CallerMemberName] string memberName = "")
		{
			SupportTypes = supportTypes;
			SourcePath = sourcePath;
			MemberName = memberName;
		}
	}
}
