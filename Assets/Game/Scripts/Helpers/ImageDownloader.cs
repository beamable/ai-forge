using System;
using System.IO;
using System.Threading.Tasks;
using Beamable.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Helpers
{
    public class ImageDownloader
    {
        public static string BasePath => Path.Combine(Application.persistentDataPath, "imgs");

        public static async Task<Texture2D> GetImage(string url, string imageId)
        {
#if !UNITY_WEBGL
            if (ExistLocally(imageId))
            {
                Debug.Log($"Get image locally {imageId}: {url}");
                var fileData = await File.ReadAllBytesAsync(ImagePath(imageId));
                var tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                return tex;
            }
#endif
            var remoteTexture = await GetRemoteTexture(url);
#if !UNITY_WEBGL
            await SaveTexture(remoteTexture, imageId);
#endif
            return remoteTexture;
        }

        private static async Task SaveTexture(Texture2D texture, string imageId)
        {
            byte[] bytes = texture.EncodeToPNG();
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            await File.WriteAllBytesAsync(ImagePath(imageId), bytes);
        }

        private static bool ExistLocally(string imageId) => File.Exists(ImagePath(imageId));

        private static string ImagePath(string imageId) =>
            Path.Combine(BasePath, imageId);

        public static Promise<Texture2D> GetRemoteTexture(string url)
        {
            Debug.Log($"Request for {url}");
            var promise = new Promise<Texture2D>();
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            // begin request:
            var asyncOp = www.SendWebRequest();
            asyncOp.completed += _ =>
            {
                // read results:
                Debug.Log($"Request completed for {url}");
                if (www.result != UnityWebRequest.Result.Success) // for Unity >= 2020.1
                {
                    promise.CompleteError(new Exception($"{www.error}, URL:{www.url}"));
                }
                else
                {
                    promise.CompleteSuccess(DownloadHandlerTexture.GetContent(www));
                }

                www.Dispose();
            };
            return promise;
        }
    }
}