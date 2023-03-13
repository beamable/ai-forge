using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Editor.Environment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEngine;

public class BeamableEditorWebRequester : IEditorHttpRequester
{
	public async Promise<T> ManualRequest<T>(Method method,
											 string url,
											 object body = null,
											 Dictionary<string, string> headers = null,
											 string contentType = "application/json",
											 Func<string, T> parser = null)
	{
		if (body != null)
			throw new NotImplementedException();

		var client = new HttpClient();
		HttpMethod httpMethod;
		switch (method)
		{
			case Method.DELETE:
				httpMethod = HttpMethod.Delete;
				break;
			case Method.POST:
				httpMethod = HttpMethod.Post;
				break;
			case Method.PUT:
				httpMethod = HttpMethod.Put;
				break;
			default:
			case Method.GET:
				httpMethod = HttpMethod.Get;
				break;
		}

		var clientRequest = new HttpRequestMessage(httpMethod, url);
		if (headers != null)
		{
			var headerCollection = new WebHeaderCollection();

			foreach (var header in headers)
			{
				headerCollection.Add(header.Key, header.Value);
				client.DefaultRequestHeaders.Add(header.Key, header.Value);
			}
		}
		var httpResponse = await client.SendAsync(clientRequest);
		var responsePayload = await httpResponse.Content.ReadAsStringAsync();
		T parsedResult = parser == null
			? JsonUtility.FromJson<T>(responsePayload)
			: parser(responsePayload);

		return parsedResult;
	}

}
