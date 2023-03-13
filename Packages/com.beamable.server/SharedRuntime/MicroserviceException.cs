using Beamable.Common;
using Beamable.Common.Api;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Server
{
	public class MicroserviceException : RequesterException
	{
		public int ResponseStatus { get; set; }
		public string Error { get; set; }
		public new string Message { get; set; }

		public MicroserviceException(int responseStatus, string error, string message) : base(Constants.Requester.ERROR_PREFIX_MICROSERVICE, error, string.Empty, responseStatus, new BeamableRequestError
		{
			message = message,
			error = error,
			service = "beam-microservice",
			status = responseStatus
		})
		{
			ResponseStatus = responseStatus;
			Error = error;
			Message = message;
		}
	}

	public class SocketClosedException : MicroserviceException
	{
		public List<Exception> FailedAttemptExceptions { get; }

		public SocketClosedException(List<Exception> failedAttemptExceptions) : base(500, "socket", "the socket is closed. Too many retries have happened, and the message cannot be sent. internal errors=" + string.Join("\n", failedAttemptExceptions.Select(x => x?.Message)))
		{
			FailedAttemptExceptions = failedAttemptExceptions;
		}
	}

	public class MissingScopesException : MicroserviceException
	{
		public MissingScopesException(IEnumerable<string> currentScopes)
		: base(403, "invalidScopes", $"The scopes [{string.Join(",", currentScopes)}] aren't sufficient for the request.")
		{

		}
	}

	public class UnauthorizedUserException : MicroserviceException
	{
		public UnauthorizedUserException(string methodPath)
			: base(401, "unauthorizedUser", $"The request to [{methodPath}] requires an authenticated user.")
		{

		}
	}

	public class UnhandledPathException : MicroserviceException
	{
		public UnhandledPathException(string path)
		: base(404, "unhandledRoute", $"The path=[{path}] has no handler")
		{

		}
	}

	public class ParameterCardinalityException : MicroserviceException
	{
		public ParameterCardinalityException(int requiredCount, int actualCount)
		: base(400, "inputParameterFailure", $"Parameter cardinality failure. required={requiredCount} given={actualCount}")
		{

		}
	}

	public class ParameterLegacyException : MicroserviceException
	{
		public ParameterLegacyException()
		   : base(400, "inputParameterFailure", $"Parameters could not be resolved due to legacy reasons. Please don't use the parameter name, \"payload\". Consider using the [Parameter] attribute to rename the parameter. ")
		{

		}
	}

	public class ParameterMissingRequiredException : MicroserviceException
	{
		public ParameterMissingRequiredException(string missingParameterName)
		   : base(400, "inputParameterFailure", $"Parameter requires property={missingParameterName}")
		{

		}
	}

	public class ParameterNullException : MicroserviceException
	{
		public ParameterNullException()
		   : base(400, "inputParameterFailure", $"Parameters payload cannot be null. Use an empty array for no parameters.")
		{

		}
	}

	public class BadInputException : MicroserviceException
	{
		public BadInputException(string payload, Exception inner)
			: base(400, "inputParameterFailure", $"Your input failed to deserialize correctly. Payload=[{payload}] Inner message=[{inner?.Message ?? "?"}]")
		{

		}
	}
}
