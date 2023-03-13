using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine.Networking;

namespace Core.Platform.SDK
{
	public static class Gzip
	{
		public const int MINIMUM_BYTES_FOR_COMPRESSION = 1000;

		public static byte[] Compress(string data)
		{
			var dataBytes = Encoding.UTF8.GetBytes(data);
			return Compress(dataBytes);
		}

		public static byte[] Compress(byte[] data)
		{
			using (var inputStream = new MemoryStream(data))
			using (var outputStream = new MemoryStream())
			{
				using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
				{
					inputStream.CopyTo(gzipStream);
				}
				return outputStream.ToArray();
			}
		}

		public static string Decompress(byte[] data)
		{
			using (var inputStream = new MemoryStream(data))
			using (var outputStream = new MemoryStream())
			{
				using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
				{
					gzipStream.CopyTo(outputStream);
				}

				return Encoding.UTF8.GetString(outputStream.ToArray());
			}
		}
	}

	public static class GzipWebExtensions
	{
		public static void SetRequestCompressionHeader(this UnityWebRequest request) => request.SetRequestHeader("Content-Encoding", "gzip");
	}

}
