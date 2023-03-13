using Beamable.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using static Beamable.Common.Constants.Features.LoginBase;

namespace Beamable.Editor.Login.UI
{
	public class LoginErrorHandlers
	{
		public delegate bool Predicate(PlatformRequesterException err);

		private readonly Dictionary<Predicate, string> _predicateToErrorGenerator = new Dictionary<Predicate, string>();

		private string _unknownMessage;

		public LoginErrorHandlers OnUnknown(string message)
		{
			_unknownMessage = message;
			return this;
		}

		public LoginErrorHandlers On(Predicate predicate, string message)
		{
			_predicateToErrorGenerator.Add(predicate, message);
			return this;
		}

		public LoginErrorHandlers OnStatus(long status, string message)
		{
			return On(err => err.Status == status, message);
		}

		public LoginErrorHandlers OnStatusAndError(long status, string error, string errorMessage)
		{
			return On(err => err.Status == status && string.Equals(err.Error.error, error), errorMessage);
		}

		public LoginErrorHandlers OnBadRequest(string message) => OnStatus(400, message);
		public LoginErrorHandlers OnBadRequest(string error, string message) => OnStatusAndError(400, error, message);
		public LoginErrorHandlers OnNotFound(string message) => OnStatus(404, message);
		public LoginErrorHandlers OnUnauthorized(string message) => OnStatus(401, message);
		public LoginErrorHandlers OnForbidden(string message) => OnStatus(403, message);
		public LoginErrorHandlers OnServerError(string message) => OnStatus(500, message);

		public string ProduceError(Exception ex)
		{
			string message = _unknownMessage;
			if (ex is PlatformRequesterException err && _predicateToErrorGenerator.TryGetValue(_predicateToErrorGenerator.Keys.FirstOrDefault(p => p(err)) ?? (_ => false), out var errorMessage))
			{
				message = errorMessage;
			}

			return message ?? UNKNOWN_ERROR;
		}
	}
}
