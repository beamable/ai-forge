using System.Text;
using static System.Diagnostics.Debug;

namespace Beamable.Common.Assistant
{
	/// <summary>
	/// <see cref="BeamHint"/> ids are unique identifiers that translates to a single displayed hint and for which notifications are tracked.
	/// <para/>
	/// There are 3 primary ways of thinking about ids:
	/// <list type="bullet">
	/// <item><description>
	/// Identify several instances of the same problem and display all of them in a single consistent
	/// hint (see <see cref="BeamHintDomains.BEAM_CSHARP_MICROSERVICES_CODE_MISUSE"/>, or any other Code Misuse, as examples).
	/// </description></item>
	/// <item><description>
	/// Identify a single hint/tip that may be interesting to communicate and create a hint Id for it.
	/// </description></item>
	/// <item><description>
	/// Embed a content/player or some other unique identifier into the id to guarantee uniqueness and display relevant data.
	/// Be careful with this approach as it can easily flood the Assistant window making it less useful.
	/// </description></item>
	/// </list>
	/// <para/>
	/// Ids cannot have "¬" or "₢" they are reserved characters (see <see cref="BeamHintDomains.SUB_DOMAIN_SEPARATOR"/>, <see cref="BeamHintHeader.AS_KEY_SEPARATOR"/> and <see cref="BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR"/>).
	/// <para/>
	/// Use <see cref="GenerateHintId"/> to ensure the immutable part of the hint's id is valid when declaring it.
	/// <para/>
	/// Use <see cref="AppendHintIdParams"/> methods to ensure you are respecting the Id creation rules and you are embedding parameters into the id (for cases like the third item in the bullet point list). 
	/// </summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	public class BeamHintIds : BeamHintIdProvider
	{
		/// <summary>
		/// Generates a given <paramref name="id"/>. Asserts it is a valid id as per system assumptions.
		/// </summary>
		public static string GenerateHintId(string id, string prefix = null)
		{
			Assert(!id.Contains(BeamHintDomains.SUB_DOMAIN_SEPARATOR) &&
				   !id.Contains(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR),
				$"Failed to generate hint id {id}! The Id cannot contain {BeamHintDomains.SUB_DOMAIN_SEPARATOR} or {BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR}");

			return prefix != null ? $"{prefix}-{id}" : id;
		}

		/// <summary>
		/// Given a hint generated with <see cref="GenerateHintId"/>, appends -- split by the given separator -- the given list of parameters
		/// (calling <see cref="object.ToString"/> to generate them).
		/// </summary>
		public static string AppendHintIdParams(string id, string separator = "_", params object[] appendedParams)
		{
			Assert(!id.Contains(BeamHintDomains.SUB_DOMAIN_SEPARATOR) &&
				   !id.Contains(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR),
				   $"Failed to generate hint id with id={id}! The Id cannot contain {BeamHintDomains.SUB_DOMAIN_SEPARATOR} or {BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR}");

			Assert(!separator.Contains(BeamHintDomains.SUB_DOMAIN_SEPARATOR) &&
				   !separator.Contains(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR),
				   $"Failed to generate hint id with id={id} and separator={separator}! The separator cannot contain {BeamHintDomains.SUB_DOMAIN_SEPARATOR} or {BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR}");

			foreach (var param in appendedParams)
			{
				Assert(!param.ToString().Contains(BeamHintDomains.SUB_DOMAIN_SEPARATOR) &&
					   !param.ToString().Contains(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR),
					   $"Failed to generate hint id with id={id}, separator={separator} and param={param}! The param cannot contain {BeamHintDomains.SUB_DOMAIN_SEPARATOR} or {BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR}");
			}

			var idBuilder = new StringBuilder($"{id}");
			foreach (var param in appendedParams)
			{
				idBuilder.Append($"{separator}{param}");
			}

			return idBuilder.ToString();
		}

		/// <summary>
		/// Prefix added to all hints whose context objects are known to be lists of <see cref="Reflection.AttributeValidationResult"/>.
		/// </summary>
		public const string ATTRIBUTE_VALIDATION_ID_PREFIX = "AttributeValidation";

		/// <summary>
		/// Prefix added to all hints whose context objects are known to be lists of <see cref="Reflection.UniqueNameCollisionData"/>.
		/// </summary>
		public const string ATTRIBUTE_NAME_COLLISION_ID_PREFIX = "AttributeNameCollision";



		// Beamable Initialization IDs
		[BeamHintId] public static readonly string ID_UNSUPPORTED_REGISTER_BEAMABLE_DEPENDENCY_SIGNATURE = GenerateHintId("UnsupportedRegisterBeamableDependencySignature", ATTRIBUTE_VALIDATION_ID_PREFIX);

		// Beamable Assistant IDs
		[BeamHintId] public static readonly string ID_MISCONFIGURED_HINT_DETAILS_PROVIDER = GenerateHintId("MisconfiguredHintDetailsProvider", ATTRIBUTE_VALIDATION_ID_PREFIX);
		[BeamHintId] public static readonly string ID_MISCONFIGURED_HINT_SYSTEM_ATTRIBUTE = GenerateHintId("MisconfiguredHintSystemAttribute", ATTRIBUTE_VALIDATION_ID_PREFIX);

		// Content - Code Misuse IDs
		[BeamHintId] public static readonly string ID_CONTENT_TYPE_ATTRIBUTE_MISSING = GenerateHintId("ContentTypeAttributeMissing", ATTRIBUTE_VALIDATION_ID_PREFIX);
		[BeamHintId] public static readonly string ID_CONTENT_TYPE_NAME_COLLISION = GenerateHintId("ContentTypeNameCollision", ATTRIBUTE_NAME_COLLISION_ID_PREFIX);
		[BeamHintId] public static readonly string ID_INVALID_CONTENT_TYPE_ATTRIBUTE = GenerateHintId("InvalidContentTypeAttribute", ATTRIBUTE_VALIDATION_ID_PREFIX);
		[BeamHintId] public static readonly string ID_INVALID_CONTENT_FORMERLY_SERIALIZED_AS_ATTRIBUTE = GenerateHintId("InvalidContentFormerlySerializedAsAttribute", ATTRIBUTE_VALIDATION_ID_PREFIX);

		// Microservices - Code Misuse IDs
		[BeamHintId] public static readonly string ID_MICROSERVICE_ATTRIBUTE_MISSING = GenerateHintId("MicroserviceAttributeMissing", ATTRIBUTE_VALIDATION_ID_PREFIX);
		[BeamHintId] public static readonly string ID_MICROSERVICE_NAME_COLLISION = GenerateHintId("MicroserviceNameCollision", ATTRIBUTE_NAME_COLLISION_ID_PREFIX);
		[BeamHintId] public static readonly string ID_STORAGE_OBJECT_ATTRIBUTE_MISSING = GenerateHintId("StorageObjectAttributeMissing", ATTRIBUTE_VALIDATION_ID_PREFIX);
		[BeamHintId] public static readonly string ID_STORAGE_OBJECT_NAME_COLLISION = GenerateHintId("StorageObjectNameCollision", ATTRIBUTE_NAME_COLLISION_ID_PREFIX);
		[BeamHintId] public static readonly string ID_CLIENT_CALLABLE_ASYNC_VOID = GenerateHintId("ClientCallableAsyncVoid", ATTRIBUTE_VALIDATION_ID_PREFIX);
		[BeamHintId] public static readonly string ID_CLIENT_CALLABLE_UNSUPPORTED_PARAMETERS = GenerateHintId("ClientCallableUnsupportedParameters", ATTRIBUTE_VALIDATION_ID_PREFIX);

		// Microservices - Docker - Ids
		[BeamHintId] public static readonly string ID_INSTALL_DOCKER_PROCESS = GenerateHintId("InstallDockerProcess");
		[BeamHintId] public static readonly string ID_DOCKER_PROCESS_NOT_RUNNING = GenerateHintId("DockerProcessNotRunning");
		[BeamHintId] public static readonly string ID_DOCKER_OVERLAPPING_PORTS = GenerateHintId("DockerOverlappingPorts");

		[BeamHintId] public static readonly string ID_CHANGES_NOT_DEPLOYED_TO_LOCAL_DOCKER = GenerateHintId("ChangesNotDeployedToLocalDocker");

	}
}
