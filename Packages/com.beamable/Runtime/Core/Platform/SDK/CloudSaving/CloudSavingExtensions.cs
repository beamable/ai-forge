using Beamable.Common;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Beamable.Api.CloudSaving
{
	public static class CloudSavingExtensions
	{
		public class CloudFile<T>
		{
			public bool Exists { get; set; }
			public T Data { get; set; }
		}

		public static string GetLocalizedPath(this CloudSavingService service, string filePath)
		{
			var path = Path.Combine(service.LocalCloudDataFullPath, filePath);
			return path;
		}
		public static Promise<Unit> WriteFileBytes(this CloudSavingService service, string filePath, byte[] fileContent)
		{
			return service.Init().Map(_ =>
			{
				var path = GetLocalizedPath(service, filePath);
				File.WriteAllBytes(path, fileContent);
				return PromiseBase.Unit;
			});
		}

		public static Promise<CloudFile<byte[]>> ReadFileBytes(this CloudSavingService service, string filePath)
		{
			return service.Init().Map(_ =>
			{
				var path = GetLocalizedPath(service, filePath);
				var exists = File.Exists(path);
				var bytes = exists
				? File.ReadAllBytes(path)
				: new byte[] { };
				return new CloudFile<byte[]>
				{
					Exists = exists,
					Data = bytes
				};
			});
		}

		public static Promise<Unit> WriteFileString(this CloudSavingService service, string filePath, string fileContent)
		{
			return WriteFileBytes(service, filePath, Encoding.ASCII.GetBytes(fileContent));
		}

		public static Promise<CloudFile<string>> ReadFileString(this CloudSavingService service, string filePath)
		{
			return ReadFileBytes(service, filePath).Map(data => new CloudFile<string>
			{
				Exists = data.Exists,
				Data = Encoding.ASCII.GetString(data.Data)
			});
		}

		public static Promise<Unit> Write<T>(this CloudSavingService service, string filePath, T content, Func<T, string> serializer = null)
		{
			if (serializer == null) serializer = (raw) => JsonUtility.ToJson(raw);

			var serialized = serializer(content);
			return WriteFileString(service, filePath, serialized);
		}

		public static Promise<CloudFile<T>> Read<T>(this CloudSavingService service, string filePath, Func<string, T> deserializer = null)
		{
			if (deserializer == null) deserializer = JsonUtility.FromJson<T>;
			return ReadFileString(service, filePath).Map(data => new CloudFile<T>
			{
				Exists = data.Exists,
				Data = data.Exists ? deserializer(data.Data) : default
			});

		}
	}
}
