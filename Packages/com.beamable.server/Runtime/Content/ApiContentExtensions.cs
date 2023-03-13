using Beamable.Common;
using Beamable.Common.Content;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Beamable.Server
{
	public static class ApiContentExtensions
	{
		private static readonly ApiVariableBag EMPTY_VARIABLES = new ApiVariableBag();

		/// <summary>
		/// Invoke the api callback for some <see cref="ApiRef"/> with some variables.
		/// This call will go to your local running microservice if its running else the remote microservice.
		///
		/// If you are running this from Unity, <b> This call won't have admin privledges </b> because it is originating from client code.
		/// However, if you are executing this method from a Microservice context, the call <i>will</i> have admin privs.
		/// </summary>
		/// <param name="apiRef"></param>
		/// <param name="variables">Optionally, you can pass variables to this call. If the api callback requires variables, and you don't pass sufficient variables, the call will fail.</param>
		public static async Promise RequestApi<T>(this ApiRef<T> apiRef, ApiVariableBag variables = null)
			where T : ApiContent, new()
		{
			var content = await apiRef.Resolve();
			await RequestApi(content, variables);
		}

		/// <summary>
		/// Invoke the api callback for some <see cref="ApiContent"/> with some variables.
		/// This call will go to your local running microservice if its running else the remote microservice.
		///
		/// If you are running this from Unity, <b> This call won't have admin privledges </b> because it is originating from client code.
		/// However, if you are executing this method from a Microservice context, the call <i>will</i> have admin privs.
		/// </summary>
		/// <param name="api"></param>
		/// <param name="variables">Optionally, you can pass variables to this call. If the api callback requires variables, and you don't pass sufficient variables, the call will fail.</param>
		public static async Promise RequestApi(this ApiContent api, ApiVariableBag variables = null)
		{
			// TODO: all people to configure which requester gets used
			var requester = await Beamable.API.Instance.Map(b => b.Requester);
			await MicroserviceClientHelper.Request<Unit>(requester,
														 api.ServiceRoute.Service,
														 api.ServiceRoute.Endpoint,
														 api.PrepareParameters(variables));
		}


		private static string[] PrepareParameters(this ApiContent content, ApiVariableBag variableBag = null)
		{
			variableBag = variableBag ?? EMPTY_VARIABLES;

			var parameters = content.Parameters.Parameters;

			var outputs = new string[parameters.Length];

			for (var i = 0; i < outputs.Length; i++)
			{
				outputs[i] = parameters[i].ResolveParameter(variableBag);
				outputs[i] = Regex.Unescape(outputs[i]);
			}
			return outputs;
		}

		private static string ResolveParameter(this RouteParameter parameter, ApiVariableBag variables = null)
		{
			variables = variables ?? EMPTY_VARIABLES;
			if (parameter.variableReference.HasValue)
			{
				var key = parameter.variableReference.Value.Name;
				if (variables.TryGetValue(key, out var raw))
				{
					return MicroserviceClientHelper.SerializeArgument(raw);
				}
				else
				{
					Debug.LogError($"There is no variable for {key}");
					return null;
				}
			}

			return parameter.Data;

		}
	}

}
