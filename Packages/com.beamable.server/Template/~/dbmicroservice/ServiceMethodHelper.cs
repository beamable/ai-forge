using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beamable.Server
{
	public static class ServiceMethodHelper
	{
		public static ServiceMethodCollection Scan<TMicroService>() where TMicroService : MicroService
		{
			var output = new List<ServiceMethod>();
			var type = typeof(TMicroService); // rely on dynamic

			Console.WriteLine($"Scanning for methods... type=[{type.Name}]");

			var allMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
			foreach (var method in allMethods)
			{
				var closureMethod = method;
				var attribute = method.GetCustomAttribute<ClientCallable>();
				if (attribute == null) continue;

				var servicePath = attribute.PathName;
				if (string.IsNullOrEmpty(servicePath))
				{
					servicePath = method.Name;
				}

				Console.WriteLine($"Found method=[{method.Name}] path={servicePath}");

				var parameters = method.GetParameters();

				var deserializers = new List<Func<string, object>>();
				foreach (var parameter in parameters)
				{
					var pType = parameter.ParameterType;

					Func<string, object> deserializer = (json) =>
					{
						var deserializeObject = JsonConvert.DeserializeObject(json, pType);
						return deserializeObject;
					};
					deserializers.Add(deserializer);
				}

				var isAsync = null != method.GetCustomAttribute<AsyncStateMachineAttribute>();

				Func<object, object[], Task> executor = (target, args) =>
				{
					var invocationResult = closureMethod.Invoke(target, args);
					return Task.FromResult(invocationResult);
				};

				if (isAsync)
				{
					executor = (target, args) =>
					{
						var task = (Task)closureMethod.Invoke(target, args);
						return task;
					};
				}

				var serviceMethod = new ServiceMethod
				{
					Path = servicePath,
					Method = method,
					Deserializers = deserializers,
					Executor = executor
				};
				output.Add(serviceMethod);
			}

			return new ServiceMethodCollection(output);
		}

	}
}
