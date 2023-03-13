using Beamable.Common;
using Beamable.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Beamable.Server
{
	/// <summary>
	/// Base callable attribute used to identify methods to be exposed by microservices as endpoints. This attribute makes the endpoint publicly accessible (no need for authentication).
	/// <see cref="ClientCallableAttribute"/> forces the authentication to be required.
	/// <see cref="AdminOnlyCallableAttribute"/> makes it so that only an admin/developer can reach the endpoint. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class CallableAttribute : Attribute, INamingAttribute
	{
		public static readonly List<ParameterOfInterest> UNSUPPORTED_PARAMETER_TYPES = new List<ParameterOfInterest>()
		{
			new ParameterOfInterest(typeof(Delegate), false, false, false),
			new ParameterOfInterest(typeof(Task), false, false, false),
			new ParameterOfInterest(typeof(Promise), false, false, false),
		};

		protected string pathName = "";
		public HashSet<string> RequiredScopes { get; }

		public bool RequireAuthenticatedUser { get; }

		public CallableAttribute() : this("", null, false) { }

		public CallableAttribute(string pathnameOverride = "", string[] requiredScopes = null, bool requireAuthenticatedUser = false)
		{
			pathName = pathnameOverride;
			RequiredScopes = requiredScopes == null
				? new HashSet<string>()
				: new HashSet<string>(requiredScopes);

			RequireAuthenticatedUser = requireAuthenticatedUser;
		}

		public string PathName
		{
			set => pathName = value;
			get => pathName;
		}

		public string[] Names => new[] { pathName };

		public virtual AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			var methodInfo = (MethodInfo)member;

			// Check for any unsupported parameter types.
			if (UNSUPPORTED_PARAMETER_TYPES.MatchAnyParametersOfMethod(methodInfo, out var detectedUnsupportedTypes))
			{
				var message = $"The unsupported parameters are: {string.Join(", ", detectedUnsupportedTypes.Select(p => $"{p.ParameterType.Name} {p.Name}"))}";
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Error, message);
			}

			// Check for void signatures to send out warning.
			if (methodInfo.IsAsyncMethodOfType(typeof(void)))
			{
				var message = $"";
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Warning, message);
			}

			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, $"");
		}

		public virtual AttributeValidationResult AreValidNameForType(MemberInfo member, string[] potentialNames)
		{
			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, $"");
		}
	}

	/// <summary>
	/// This type defines the %Microservice method attribute for any
	/// %Microservice method which can be called EITHER from the %Client or
	/// a %Microservice by a User account of any type.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Server.Microservice script reference
	/// - See Beamable.Server.AdminOnlyCallableAttribute script reference
	///
	/// ### Example
	/// This demonstrates example usage from WITHIN a custom %Beamable %Microservice.
	///
	/// ```
	/// [ClientCallable]
	/// private async void MyMicroserviceMethod()
	/// {
	///
	///   // Do something...
	/// 
	/// }
	///
	/// ```
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ClientCallableAttribute : CallableAttribute
	{
		public ClientCallableAttribute() : this("", null) { }
		public ClientCallableAttribute(string pathnameOverride = "", string[] requiredScopes = null) : base(pathnameOverride, requiredScopes, true) { }
	}

	/// <summary>
	/// This type defines the %Microservice method attribute for any
	/// %Microservice method which can be called ONLY from a
	/// %Microservice by a User account of %Admin type.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Server.Microservice script reference
	/// - See Beamable.Server.ClientCallableAttribute script reference
	///
	/// ### Example
	/// This demonstrates example usage from WITHIN a custom %Beamable %Microservice.
	///
	/// ```
	/// [AdminOnlyCallable]
	/// private async void MyMicroserviceMethod()
	/// {
	///
	///   // Do something...
	/// 
	/// }
	///
	/// ```
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class AdminOnlyCallableAttribute : ClientCallableAttribute
	{
		public AdminOnlyCallableAttribute(string pathnameOverride = "") : base(pathnameOverride,
																			   requiredScopes: new[] { "*" })
		{ }
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class CustomResponseSerializationAttribute : Attribute
	{
		public virtual string SerializeResponse(object raw)
		{
			return raw.ToString();
		}
	}
}
