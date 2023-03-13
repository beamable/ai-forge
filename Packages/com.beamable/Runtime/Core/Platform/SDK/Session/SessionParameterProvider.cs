using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Api.Sessions
{
	public class SessionParameterProvider : ScriptableObject
	{
		/// <summary>
		/// Override this method to create a custom locale
		/// </summary>
		/// <returns></returns>
		public virtual Promise<string> GetCustomLocale()
		{
			return Promise<string>.Successful(SessionServiceHelper.GetISO639CountryCodeFromSystemLanguage());
		}

		public Promise<ArrayDict> GetCustomParameters(ArrayDict startingParameters, User user)
		{
			try
			{
				var customParams = new ArrayDict();

				var dict = new Dictionary<string, string>();
				foreach (var kvp in startingParameters)
				{
					dict.Add(kvp.Key, kvp.Value.ToString());
				}
				AddCustomParameters(dict, user);
				return AddCustomParametersAsync(dict, user).Map(_ =>
				{
					foreach (var kvp in dict)
					{
						customParams[kvp.Key] = kvp.Value;
					}
					return customParams;
				});
			}
			catch (Exception ex)
			{
				Debug.LogError($"Failed to create custom session parameters. {ex.Message}");
				Debug.LogException(ex);
				return Promise<ArrayDict>.Failed(ex);
			}
		}

		public virtual void AddCustomParameters(Dictionary<string, string> parameters, User user)
		{

		}

		public virtual Promise<Unit> AddCustomParametersAsync(Dictionary<string, string> customParameters, User user)
		{
			return PromiseBase.SuccessfulUnit;
		}

	}
}
