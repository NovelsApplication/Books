using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Shared.LocalCache 
{
    public static class Cacher
    {
        public static async UniTask<string> GetTextAsync(this string fileName) 
        {
            DirtyHackWithPlayerPrefs();

            var versionedFileName = fileName;
            return IsCached(versionedFileName) ?
                TextFromCache(versionedFileName) :
                ToCache(await new AssetRequests().GetText(versionedFileName), versionedFileName);
        }

        public static async UniTask<Texture2D> GetTextureAsync(this string fileName) 
        {
            DirtyHackWithPlayerPrefs();

            var versionedFileName = fileName;
            return IsCached(versionedFileName) ?
                TextureFromCache(versionedFileName) :
                ToCache(await new AssetRequests().GetTexture(versionedFileName), versionedFileName);
        }

        private static bool IsCached(this string fileName) 
        {
            var file = ConvertPath(fileName);
            return File.Exists(file);
        }

        private static Texture2D TextureFromCache(this string fileName) 
        {
            var rawData = FromCache(fileName);
            Texture2D image = new(0, 0);
            ImageConversion.LoadImage(image, rawData);
            return image;
        }

        private static string TextFromCache(this string fileName) 
        {
            var rawData = FromCache(fileName);
            return Encoding.UTF8.GetString(rawData);
        }

        private static byte[] FromCache(this string fileName) 
        {
            var file = ConvertPath(fileName);

            using (var fs = File.OpenRead(file)) 
            {
                var buffer = new byte[(int)fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        private static Texture2D ToCache(this Texture2D data, string fileName) 
        {
            var rawData = data.EncodeToPNG();
            rawData.ToCache(fileName);
            return fileName.TextureFromCache();
        }

        private static string ToCache(this string data, string fileName) 
        {
            var rawData = Encoding.UTF8.GetBytes(data);
            rawData.ToCache(fileName);
            return fileName.TextFromCache();
        }

        private static byte[] ToCache(this byte[] data, string fileName) 
        {
            var file = ConvertPath(fileName);
            if (File.Exists(file))
                File.Delete(file);
            using (var fs = File.Create(file))
            {
                fs.Write(data, 0, data.Length);
            }

            return data;
        }

        private static void DirtyHackWithPlayerPrefs() 
        {
            PlayerPrefs.SetString("Cacher", DateTime.UtcNow.ToString());
        }

        private static string ConvertPath(string fileName) 
        {
            var localFilesPath = $"{Application.persistentDataPath}/CachedFiles";
#if !UNITY_EDITOR && UNITY_WEBGL
            localFilesPath = "idbfs/CachedFiles";
#endif

            if (!Directory.Exists(localFilesPath))
                Directory.CreateDirectory(localFilesPath);

            var localExtraPath = fileName.Split('/');
            for (var  i = 0; i < localExtraPath.Length - 1; i++) 
            {
                localFilesPath += "/" + localExtraPath[i];
                if (!Directory.Exists(localFilesPath))
                    Directory.CreateDirectory(localFilesPath);
            }

            return $"{localFilesPath}/{localExtraPath.Last()}";
        }
    }
}

