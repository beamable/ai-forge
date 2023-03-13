using Beamable.Common;
using Beamable.Common.Api;
using System;
using System.Collections.Generic;

namespace Beamable.Editor.Environment
{
	public interface IEditorHttpRequester
	{
		/// <summary>
		/// Make an Editor HTTP request to any address with no preconfigured authorization
		/// </summary>
		/// <param name="method">One of the common HTTP methods represented through the <see cref="Method"/> enum</param>
		/// <param name="url"></param>
		/// <param name="body">
		/// The body of the network request.
		/// If the type of the <see cref="body"/> is an object, it will be serialized to JSON.
		/// If the type of the <see cref="body"/> is a string, no serialization will occur.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">thrown when the request result code is unknown</exception>
		/// <exception cref="NotImplementedException">thrown when the request param is set but not implemented</exception>
		/// <param name="headers">
		/// A dictionary where the keys are header names, and the values are the header values.
		/// </param>
		/// <param name="contentType">
		/// A MIME type for the request. For example, "application/json".
		/// If the <see cref="body"/> is serialized to JSON, the <see cref="contentType"/> will become "application/json" by default.
		/// </param>
		/// <param name="parser">
		/// By default, the network response will be assumed JSON and deserialized as such. However, if a <see cref="parser"/>
		/// is provided, the network response will be given the parser as a string. The parser can convert the string into the
		/// expected result type, <see cref="T"/>
		/// </param>
		/// <typeparam name="T">
		/// The type of the network response. The network response will be deserialized into an instance of <see cref="T"/>.
		/// You can override the parsing by passing a custom <see cref="parser"/>
		/// </typeparam>
		/// <returns>
		/// A <see cref="Promise{T}"/> of type <see cref="T"/> when the network request completes.
		/// </returns>
		Promise<T> ManualRequest<T>(Method method,
									string url,
									object body = null,
									Dictionary<string, string> headers = null,
									string contentType = "application/json",
									Func<string, T> parser = null);

	}
}
