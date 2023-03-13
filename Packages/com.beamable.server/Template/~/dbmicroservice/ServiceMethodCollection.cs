using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beamable.Server
{
	public class ServiceMethodCollection
	{
		private Dictionary<string, ServiceMethod> _pathToMethod;

		public ServiceMethodCollection(IEnumerable<ServiceMethod> methods)
		{
			_pathToMethod = methods.ToDictionary(method => method.Path);
		}

		public async Task<string> Handle(MicroService service, string path, string[] jsonArgs)
		{
			Console.WriteLine("Handling path... " + path);
			if (_pathToMethod.TryGetValue(path, out var method))
			{
				var output = method.Execute(service, jsonArgs);
				var result = await output;
				Console.WriteLine($"Ran method. out=[{result}]");

				return result;
			}
			else
			{
				Console.WriteLine("No handler found");
				foreach (var kvp in _pathToMethod)
				{
					Console.WriteLine("Did you mean... " + kvp.Key);
				}
				throw new Exception($"Unhandled path=[{path}]");
			}
		}
	}
}
