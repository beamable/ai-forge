using Beamable.Editor;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Beamable.Server.Editor.Uploader
{
	public class ContainerUploader
	{
		// See https://docs.docker.com/registry/spec/manifest-v2-2/#image-manifest-field-descriptions
		private const string MediaManifest = "application/vnd.docker.distribution.manifest.v2+json";
		private const string MediaConfig = "application/vnd.docker.container.image.v1+json";
		private const string MediaLayer = "application/vnd.docker.image.rootfs.diff.tar.gzip";
		//private const int ChunkSize = 1048576; // TODO: Measure performance of different chunk sizes. ~ACM 2019-12-18
		private const int ChunkSize = 1048576 * 10;

		/// <summary>
		/// Tag to apply when uploading images.
		/// </summary>
		private const string DockerTagReference = "latest";

		private readonly HttpClient _client;
		private readonly string _uploadBaseUri;
		private readonly HashAlgorithm _sha256;
		private readonly ContainerUploadHarness _harness;
		private readonly MicroserviceDescriptor _descriptor;
		private readonly string _imageId;
		private readonly MD5 _md5 = MD5.Create();
		private long _partsCompleted;
		private long _partsAmount;

		public ContainerUploader(BeamEditorContext api, ContainerUploadHarness harness, MicroserviceDescriptor descriptor, string imageId)
		{
			_client = new HttpClient();
			_client.DefaultRequestHeaders.Add("x-ks-clientid", api.CurrentCustomer.Cid);
			_client.DefaultRequestHeaders.Add("x-ks-projectid", api.CurrentRealm.Pid);
			_client.DefaultRequestHeaders.Add("x-ks-token", api.Requester.Token.Token);
			var serviceUniqueName = GetHash($"{api.CurrentCustomer.Cid}_{api.ProductionRealm.Pid}_{descriptor.Name}").Substring(0, 30);
			_uploadBaseUri = $"{BeamableEnvironment.DockerRegistryUrl}{serviceUniqueName}";
			_sha256 = SHA256.Create();
			_harness = harness;
			_descriptor = descriptor;
			_imageId = imageId;
		}

		protected string GetHash(string input)
		{
			byte[] data = _md5.ComputeHash(Encoding.UTF8.GetBytes(input));
			var sBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}
			return sBuilder.ToString();
		}

		/// <summary>
		/// Upload a Docker image that has been expanded into a folder.
		/// </summary>
		/// <param name="folder">The filesystem path to the expanded image.</param>
		public async Task Upload(string folder, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var manifest = DockerManifest.FromBytes(File.ReadAllBytes($"{folder}/manifest.json"));
			var uploadManifest = new Dictionary<string, object>
		 {
			{"schemaVersion", 2},
			{"mediaType", MediaManifest},
			{"config", new Dictionary<string, object> {{"mediaType", MediaConfig}}},
			{"layers", new List<object>()},
//            {"tag", _imageId}
         };
			var config = (Dictionary<string, object>)uploadManifest["config"];
			var layers = (List<object>)uploadManifest["layers"];

			// Upload the config JSON as a blob.
			_partsAmount = manifest.layers.Length + 1;
			_harness.ReportUploadProgress(_descriptor.Name, 0, _partsAmount);

			_partsCompleted = 0;
			var configResult = (await UploadFileBlob($"{folder}/{manifest.config}", token));
			token.ThrowIfCancellationRequested();
			_harness.ReportUploadProgress(_descriptor.Name, ++_partsCompleted, _partsAmount);
			config["digest"] = configResult.Digest;
			config["size"] = configResult.Size;

			// Upload all layer blobs.
			var uploadIndexToJob = new SortedDictionary<int, Task<Dictionary<string, object>>>();
			for (var i = 0; i < manifest.layers.Length; i++)
			{
				var layer = manifest.layers[i];
				uploadIndexToJob.Add(i, UploadLayer($"{folder}/{layer}", token));
			}

			await Task.Run(() => Task.WhenAll(uploadIndexToJob.Values), token);
			token.ThrowIfCancellationRequested();
			foreach (var kvp in uploadIndexToJob)
			{
				layers.Add(kvp.Value.Result);
			}

			// Upload manifest JSON.
			await UploadManifestJson(uploadManifest, _imageId, token);
		}

		/// <summary>
		/// Upload the manifest JSON to complete the Docker image push.
		/// </summary>
		/// <param name="uploadManifest">Data structure containing image data.</param>
		private async Task UploadManifestJson(Dictionary<string, object> uploadManifest, string imageId, CancellationToken token)
		{
			var manifestJson = Json.Serialize(uploadManifest, new StringBuilder());
			var uri = new Uri($"{_uploadBaseUri}/manifests/{imageId}");
			var response = await SendPutRequest("uploading image manifest json", uri, () => new StringContent(manifestJson, Encoding.Default, MediaManifest), token);
			response.EnsureSuccessStatusCode();
			_harness.Log("image manifest uploaded");
		}

		private async Task<HttpResponseMessage> SendRequestWithRetries(string name, Func<Task<HttpResponseMessage>> requestGenerator, CancellationToken cancellationToken, int maxAttempts = 500)
		{
			var attemptCount = 0;
			var timeoutStatusCodes = new HttpStatusCode[]
			{
				HttpStatusCode.BadGateway, HttpStatusCode.GatewayTimeout, HttpStatusCode.GatewayTimeout
			};
			async Task<HttpResponseMessage> Attempt()
			{
				if (attemptCount++ >= maxAttempts)
				{
					throw new HttpRequestException("Request timed out, and exhausted all retries.");
				}
				cancellationToken.ThrowIfCancellationRequested();
				try
				{
					var result = await requestGenerator();
					if (timeoutStatusCodes.Contains(result.StatusCode))
					{
						// failed, try again :( 
						Debug.LogWarning(
							$"Request failed with bad status code, trying again... name=[{name}] attempt=[{attemptCount}] status=[{result.StatusCode}]");
						return await Attempt();
					}

					return result;
				}

				catch (IOException io)
				{
					Debug.LogWarning($"Request failed out due to io, trying again... name=[{name}] attempt=[{attemptCount}] message=[{io.Message}]");
					return await Attempt();
				}
				catch (HttpRequestException ex) when (ex.InnerException is IOException inner)
				{
					Debug.LogWarning($"Request failed out due to inner io, trying again... name=[{name}] attempt=[{attemptCount}] message=[{ex.Message}] inner=[{inner.Message}]");
					return await Attempt();
				}
				catch (TaskCanceledException)
				{
					Debug.LogWarning($"Request timed out, trying again... name=[{name}] attempt=[{attemptCount}]");
					return await Attempt();
				}
				catch (Exception ex)
				{
					Debug.Log("Unknown upload exception!!");
					Debug.LogException(ex);
					throw;
				}
			}

			return await Attempt();
		}

		private async Task<HttpResponseMessage> SendRequest(string name,
															Func<HttpRequestMessage> requestGenerator,
															CancellationToken token) =>
			await SendRequestWithRetries(name, () => _client.SendAsync(requestGenerator()), token);
		private async Task<HttpResponseMessage> SendPutRequest(string name, Uri uri, Func<StringContent> contentGenerator, CancellationToken token) =>
			await SendRequestWithRetries(name, () => _client.PutAsync(uri, contentGenerator()), token);
		private async Task<HttpResponseMessage> SendPostRequest(string name, Uri uri, Func<StringContent> contentGenerator, CancellationToken token) =>
			await SendRequestWithRetries(name, () => _client.PostAsync(uri, contentGenerator()), token);

		/// <summary>
		/// Upload one layer of a Docker image, adding its digest to the upload
		/// manifest when complete.
		/// </summary>
		/// <param name="layerPath">Filesystem path to the layer archive.</param>
		private async Task<Dictionary<string, object>> UploadLayer(string layerPath, CancellationToken token)
		{
			var layerDigest = await UploadFileBlob(layerPath, token);
			Interlocked.Increment(ref _partsCompleted);
			_harness.ReportUploadProgress(_descriptor.Name, Interlocked.Read(ref _partsCompleted), _partsAmount);
			return new Dictionary<string, object>
		 {
			{"digest", layerDigest.Digest},
			{"size", layerDigest.Size},
			{"mediaType", MediaLayer}
		 };
		}

		/// <summary>
		/// Upload a file blob, which may be config JSON or an image layer.
		/// </summary>
		/// <param name="filename">File to upload.</param>
		/// <returns>Hash digest of the blob.</returns>
		private async Task<FileBlobResult> UploadFileBlob(string filename, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			using (var fileStream = File.OpenRead(filename))
			{
				var digest = HashDigest(fileStream);
				if (await CheckBlobExistence(digest, token))
				{
					return new FileBlobResult
					{
						Digest = digest,
						Size = fileStream.Length
					};
				}
				fileStream.Position = 0;
				var location = NormalizeWithDigest(await PrepareUploadLocation(token), digest);
				while (fileStream.Position < fileStream.Length)
				{
					token.ThrowIfCancellationRequested();
					var chunk = await FileChunk.FromParent(fileStream, ChunkSize);
					var response = await UploadChunk(chunk, location, token);
					response.EnsureSuccessStatusCode();
					location = NormalizeWithDigest(response.Headers.Location, digest);
				}
				return new FileBlobResult
				{
					Digest = digest,
					Size = fileStream.Length
				};
			}
		}

		struct FileBlobResult
		{
			public string Digest;
			public long Size;
		}

		/// <summary>
		/// Upload a chunk of a file, using PATCH for intermediate chunks or PUT
		/// for the final chunk.
		/// </summary>
		/// <param name="chunk">File chunk including range information.</param>
		/// <param name="location">URI for upload.</param>
		/// <returns>HTTP response.</returns>
		private async Task<HttpResponseMessage> UploadChunk(FileChunk chunk, Uri location, CancellationToken token)
		{
			var uri = location;
			var method = chunk.IsLast ? HttpMethod.Put : new HttpMethod("PATCH");
			var content = new StreamContent(chunk.Stream);

			HttpRequestMessage Generator()
			{
				var request = new HttpRequestMessage(method, uri) { Content = content };
				request.Content.Headers.ContentLength = chunk.Length;
				request.Content.Headers.ContentRange = new ContentRangeHeaderValue(chunk.Start, chunk.End, chunk.FullLength);
				return request;
			}

			var response = await SendRequest("uploading chunk " + location, Generator, token);
			try
			{
				response.EnsureSuccessStatusCode();
			}
			catch (HttpRequestException ex)
			{
				try
				{
					var body = await response.Content.ReadAsStringAsync();
					Debug.LogError($"Failed to upload image chunk. message=[{ex.Message}] body=[{body}]");
				}
				catch (ObjectDisposedException disposedException)
				{
					Debug.LogError($"Failed to upload image chunk. message=[{ex.Message} Cannot display body. {disposedException.Message}]");
				}

				throw ex;
			}
			return response;
		}

		/// <summary>
		/// Check whether a blob exists using a HEAD request.
		/// </summary>
		/// <param name="digest"></param>
		/// <returns></returns>
		private async Task<bool> CheckBlobExistence(string digest, CancellationToken token)
		{
			var uri = new Uri($"{_uploadBaseUri}/blobs/{digest}");
			var response = await SendRequest("checking blob existence " + digest, () => new HttpRequestMessage(HttpMethod.Head, uri), token);
			return response.StatusCode == HttpStatusCode.OK;
		}

		/// <summary>
		/// Request an upload location for a blob by making POST request to the
		/// upload path.
		/// </summary>
		/// <returns>The upload location URI.</returns>
		private async Task<Uri> PrepareUploadLocation(CancellationToken token)
		{
			var uri = new Uri($"{_uploadBaseUri}/blobs/uploads/");
			var response = await SendPostRequest("preparing upload location", uri, () => new StringContent(""), token);
			try
			{
				response.EnsureSuccessStatusCode();
			}
			catch (HttpRequestException ex)
			{
				var body = await response.Content.ReadAsStringAsync();
				Debug.LogError($"Failed to prepare image upload location. message=[{ex.Message}] body=[{body}] url=[{uri}]");
				throw ex;
			}

			return response.Headers.Location;
		}

		/// <summary>
		/// Given an upload URI, make sure it uses secured HTTP and append the
		/// </summary>
		/// <param name="uri">Original URI.</param>
		/// <param name="digest">Content digest to add.</param>
		/// <returns>Revised URI.</returns>
		private static Uri NormalizeWithDigest(Uri uri, string digest)
		{
			// TODO: Figure out whether http->https redirect is possible without "buffering is needed" error. ~ACM 2019-12-18
			var builder = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps, Port = -1 };
			builder.Query += $"&digest={digest}";
			return builder.Uri;
		}

		/// <summary>
		/// Compute the SHA256 hash digest of the content stream.
		/// </summary>
		/// <param name="stream">Stream containing full content to be hashed.</param>
		/// <returns>Hash digest as a hexadecimal string with algorithm prefix.</returns>
		private string HashDigest(Stream stream)
		{
			// TODO: Can hash computation be async? ~ACM 2019-12-16
			// TODO: This seems CPU heavy; let's just trust the hashes from JSON. ~ACM 2019-12-18
			var sb = new StringBuilder("sha256:");
			foreach (var b in _sha256.ComputeHash(stream))
			{
				sb.Append($"{b:x2}");
			}
			return sb.ToString();
		}

		/// <summary>
		/// Docker manifest data structure, from JSON like:
		///   [{"Config":"...","RepoTags":["..."],"Layers":["...","..."]}]
		/// But the uploader does not need RepoTags so we omit it.
		/// </summary>
		[Serializable]
		private class DockerManifest
		{
			public string config;
			public string[] layers;

			/// <summary>
			/// Given JSON bytes that fit the expected Docker manifest
			/// schema, create a manifest data structure for the first
			/// manifest in the JSON.
			/// </summary>
			/// <param name="bytes">JSON data bytes.</param>
			/// <returns>Manifest data structure.</returns>
			public static DockerManifest FromBytes(byte[] bytes)
			{
				var result = new DockerManifest();
				var manifests = (List<object>)Json.Deserialize(bytes);
				var firstManifest = (IDictionary<string, object>)manifests[0];
				result.config = firstManifest["Config"].ToString();
				var layers = (List<object>)firstManifest["Layers"];
				result.layers = layers?.Select(x => x.ToString()).ToArray();
				return result;
			}
		}
	}
}
